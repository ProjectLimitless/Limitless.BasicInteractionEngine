/** 
* This file is part of Project Limitless.
* Copyright © 2017 Donovan Solms.
* Project Limitless
* https://www.projectlimitless.io
* 
* Project Limitless is free software: you can redistribute it and/or modify
* it under the terms of the Apache License Version 2.0.
* 
* You should have received a copy of the Apache License Version 2.0 with
* Project Limitless. If not, see http://www.apache.org/licenses/LICENSE-2.0.
*/

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Limitless.Runtime.Types;
using Limitless.Runtime.Interfaces;
using Limitless.Runtime.Interactions;
using Limitless.Runtime.Enums;

namespace Limitless.BasicInteractionEngine
{
    /// <summary>
    /// A Basic Interaction Engine for Project Limitless.
    /// </summary>
    public class BasicInteractionEngine : IModule, IInteractionEngine
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger _log;
        /// <summary>
        /// The basic intent extractor.
        /// </summary>
        private IntentExtractor _extractor;
        /// <summary>
        /// The registered skills.
        /// </summary>
        private Dictionary<string, Skill> _skills;

        /// <summary>
        /// Constructor with injected log.
        /// </summary>
        /// <param name="log">The logger to use</param>
        public BasicInteractionEngine(ILogger log)
        {
            _log = log;
            _extractor = new IntentExtractor(_log);
            _skills = new Dictionary<string, Skill>();


            var skill = new Skill();
            skill.Name = "Builtin Weather Skill";
            skill.Author = "Project Limitless";
            skill.ShortDescription = "A skill to check the weather";
            skill.Intent = new Intent();
            skill.Intent.Actions.Add("what");
            skill.Intent.Actions.Add("how");
            skill.Intent.Targets.Add("weather");
            skill.Intent.Targets.Add("forecast");
            skill.Binding = SkillExecutorBinding.Network;
            var executor = new NetworkExecutor();
            executor.Url = "https://www.google.com";
            executor.ValidateCertificate = false;
            skill.Executor = executor;
            skill.Help.Phrase = "weather";
            skill.Help.ExamplePhrase = "How's the weather for tomorrow morning";
            //skill.RequiredParameters.Add(new SkillParameter() { Parameter = "sugar", Type = SkillParameterType.Integer });
            RegisterSkill(skill);
        }

        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IModule.Configure(dynamic)"/>
        /// </summary>
        public void Configure(dynamic settings)
        {
            // Nothing to do here yet
        }
        
        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IModule.GetConfigurationType"/>
        /// </summary>
        /// <returns></returns>
        public Type GetConfigurationType()
        {
            return typeof(BasicInteractionEngineConfig);
        }
        
        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IInteractionEngine.ProcessInput(IOData)"/>
        /// </summary>
        public IOData ProcessInput(IOData ioData)
        {
            if (ioData.Mime == MimeType.Text)
            {
                _log.Info($"Processing text input");

                MatchedSkill matchedSkill = null;
                try
                {
                    matchedSkill = _extractor.Extract(ioData.Data, _skills);
                }
                catch (NotSupportedException)
                {
                    // TODO: I need to know the skills matched
                    // Multiple skills matched
                    return new IOData(MimeType.Text, "Multile skills have been matched");
                }
                catch (InvalidOperationException)
                {
                    // No skill matched
                    return new IOData(MimeType.Text, "No skill could be matched");
                }

                _log.Trace($"Executing using {matchedSkill.Skill.Binding} executor");
                // TODO: matchedSkill needs to be sent to the executor, with the metadata
                ((ISkillExecutor)matchedSkill.Skill.Executor).Execute(matchedSkill.Skill);
                

                // Different mime types
                return new IOData(MimeType.Text, "What");
            }
            else
            {
                throw new NotSupportedException($"The MIME type '{ioData.Mime}' is not supported by the BasicInteractionEngine");
            }
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.IInteractionEngine.RegisterSkill"/>
        /// </summary>
        public bool RegisterSkill(Skill skill)
        {
            if (_skills.ContainsKey(skill.UUID))
            {
                return false;
            }

            _skills.Add(skill.UUID, skill);
            return true;
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.IInteractionEngine.DeregisterSkill(string)"/>
        /// </summary>
        public bool DeregisterSkill(string skillUUID)
        {
            // Remove the skill then check if it is still in the list
            _skills.Remove(skillUUID);
            if (_skills.ContainsKey(skillUUID))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.IInteractionEngine.ListSkills"/>
        /// </summary>
        public List<Skill> ListSkills()
        {
            return _skills.Values.ToList();
        }
        
        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.IModule.GetTitle"/>
        /// </summary>
        public string GetTitle()
        {
            var assembly = GetType().Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            if (attribute != null)
            {
                return attribute.Title;
            }
            return "Unknown";
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.IModule.GetAuthor"/>
        /// </summary>
        public string GetAuthor()
        {
            var assembly = GetType().Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (attribute != null)
            {
                return attribute.Company;
            }
            return "Unknown";
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetVersion"/>
        /// </summary>
        public string GetVersion()
        {
            var assembly = GetType().Assembly;
            return assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.IModule.GetDescription"/>
        /// </summary>
        public string GetDescription()
        {
            var assembly = GetType().Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            if (attribute != null)
            {
                return attribute.Description;
            }
            return "Unknown";
        }
    }
}
