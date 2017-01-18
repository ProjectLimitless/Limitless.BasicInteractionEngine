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

using Limitless.Runtime.Types;
using Limitless.Runtime.Interfaces;
using System.Collections.Generic;

namespace Limitless.BasicInteractionEngine
{
    /// <summary>
    /// A Basic Interaction Engine for Project Limitless.
    /// </summary>
    public class BasicInteractionEngine : IModule, IInteractionEngine
    {
        public void Configure(dynamic settings)
        {
            throw new NotImplementedException();
        }

        public void DeregisterSkill()
        {
            throw new NotImplementedException();
        }

        public string GetAuthor()
        {
            throw new NotImplementedException();
        }

        public Type GetConfigurationType()
        {
            return typeof(BasicInteractionEngineConfig);
        }

        public string GetDescription()
        {
            throw new NotImplementedException();
        }

        public string GetTitle()
        {
            throw new NotImplementedException();
        }

        public string GetVersion()
        {
            throw new NotImplementedException();
        }

        public List<dynamic> ListSkills()
        {
            throw new NotImplementedException();
        }

        public IOData ProcessInput(IOData ioData)
        {
            throw new NotImplementedException();
        }

        public void RegisterSkill()
        {
            throw new NotImplementedException();
        }
    }
}
