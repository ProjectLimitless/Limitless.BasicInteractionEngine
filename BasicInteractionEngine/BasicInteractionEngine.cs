/** 
* This file is part of Project Limitless.
* Copyright © 2016 Donovan Solms.
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

            _skills = new Dictionary<string, Skill>();
            _log.Info("Loaded engine");
        }
        
        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IModule.Configure(dynamic)"/>
        /// </summary>
        public void Configure(dynamic settings)
        {
            // Nothing to do here yet
        }
        
        //TODO
        public Type GetConfigurationType()
        {
            return typeof(BasicInteractionEngineConfig);
        }
        
        // TODO
        public IOData ProcessInput(IOData ioData)
        {
            throw new NotImplementedException();
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
        /// <see cref="Limitless.Runtime.Interfaces.IInteractionEngine.ListSkills"/>
        /// </summary>
        public List<Skill> ListSkills()
        {
            return _skills.Values.ToList();
        }

        public void DeregisterSkill()
        {
            throw new NotImplementedException();
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
