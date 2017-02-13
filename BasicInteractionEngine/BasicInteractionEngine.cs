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
    /// A (Very) Basic Interaction Engine for Project Limitless.
    /// </summary>
    public class BasicInteractionEngine : IModule, IInteractionEngine
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger _log;
        /// <summary>
        /// The basic intent extractor.
        /// </summary>
        private readonly IntentExtractor _extractor;
        /// <summary>
        /// The registered skills.
        /// </summary>
        private readonly Dictionary<string, Skill> _skills;

        /// <summary>
        /// Creates a new instance of the engine using 
        /// the supplied <see cref="ILogger"/>.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use</param>
        public BasicInteractionEngine(ILogger log)
        {
            _log = log;
            _extractor = new IntentExtractor(_log);
            _skills = new Dictionary<string, Skill>();

            // TODO: Test
            var skill = new Skill();
            skill.UUID = "weather.builtin.ll.io";
            skill.Name = "Builtin Weather Skill";
            skill.Author = "Project Limitless";
            skill.ShortDescription = "A skill to check the weather";
            skill.Intent = new Intent();
            skill.Intent.Actions.Add("what");
            skill.Intent.Actions.Add("how");
            skill.Intent.Targets.Add("weather");
            skill.Intent.Targets.Add("forecast");
            skill.Binding = SkillExecutorBinding.Network;
            skill.Parameters.Add(new SkillParameter("day", SkillParameterType.DateRange));
            var executor = new NetworkExecutor();
            executor.Url = "https://www.postoffice.co.za";
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
        public IEnumerable<SupportedIOCombination> GetSupportedIOCombinations()
        {
            // BasicInteractionEngine only supports English text input and output
            return new List<SupportedIOCombination>(){
                new SupportedIOCombination(new MimeLanguage(MimeType.Text, "en-US"), new MimeLanguage(MimeType.Text, "en-US"))
            };
        }

        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IInteractionEngine.ProcessInput(IOData)"/>
        /// </summary>
        public IOData ProcessInput(IOData ioData)
        {
            if (ioData != null && ioData.MimeLanguage.Mime == MimeType.Text)
            {
                _log.Info($"Processing text input");

                Actionable actionable;
                try
                {
                    actionable = _extractor.Extract(ioData.Data, _skills);

                    // TODO: Check if required parameters are included for the matched skill - expand extract
                    if (actionable != null && actionable.HasMissingParameters())
                    {
                        var missingParameters = actionable.GetMissingParameters();
                        _log.Warning($"{missingParameters.Count} missing parameter(s) for skill '{actionable.Skill.Name}'");

                        // TODO: Engine should ask for missing parameter
                        return new IOData(new MimeLanguage(MimeType.Text, "en-US"), $"I'm missing {missingParameters.Count} parameters");
                    }

                }
                catch (NotSupportedException)
                {
                    // TODO: I need to know the skills matched
                    // Multiple skills matched
                    // HTTP status 300
                    return new IOData(new MimeLanguage(MimeType.Text, "en-US"), "Multile skills have been matched");
                }
                catch (InvalidOperationException)
                {
                    // No skill matched
                    return new IOData(new MimeLanguage(MimeType.Text, "en-US"), "No skill could be matched");
                }

                _log.Trace($"Executing using {actionable.Skill.Binding} executor");
                // TODO: matchedSkill needs to be sent to the executor, with the metadata
                ((ISkillExecutor)actionable.Skill.Executor).Execute(actionable);


                // Different mime types
                return new IOData(new MimeLanguage(MimeType.Text, "en-US"), "What now?");
            }
            throw new NotSupportedException($"The MIME type '{ioData.MimeLanguage.Mime}' is not supported by the BasicInteractionEngine");
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
        public bool DeregisterSkill(string skillUuid)
        {
            // Remove the skill then check if it is still in the list
            _skills.Remove(skillUuid);
            return !_skills.ContainsKey(skillUuid);
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
            return attribute != null ? attribute.Title : "Unknown";
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.IModule.GetAuthor"/>
        /// </summary>
        public string GetAuthor()
        {
            var assembly = GetType().Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            return attribute != null ? attribute.Company : "Unknown";
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interfaces.IModule.GetVersion"/>
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
            return attribute != null ? attribute.Description : "Unknown";
        }
    }
}
