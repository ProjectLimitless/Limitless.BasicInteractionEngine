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
using System.Collections.Generic;

using Chronic;

using Limitless.Runtime.Interfaces;
using Limitless.Runtime.Interactions;

namespace Limitless.BasicInteractionEngine
{
    /// <summary>
    /// Implements the extraction of intents from plain text
    /// using local and basic matching.
    /// </summary>
    public class IntentExtractor
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger _log;
        /// <summary>
        /// The date/time parser.
        /// </summary>
        private Parser _timeParser;
        
        /// <summary>
        /// Constructor with a logger.
        /// </summary>
        /// <param name="log">The logger to use</param>
        public IntentExtractor(ILogger log)
        {
            _log = log;
            _timeParser = new Parser();
        }

        /// <summary>
        /// Extracts the intent from the input text.
        /// </summary>
        /// <param name="input">The input text</param>
        /// <param name="skills">The current active skills</param>
        /// <returns>The extracted intent with metadata</returns>
        public string Extract(string input, Dictionary<string, Skill> skills)
        {
            _log.Trace($"Extracting intent from '{input}'");

            // Extract the dates.
            Span timeSpan = _timeParser.Parse(input);
            if (timeSpan != null)
            {
                _log.Trace($"Parser: {timeSpan.Start}");
                _log.Trace($"Parser: {timeSpan.End}");
                _log.Trace($"Parser: {timeSpan.Width}");
            }

            _log.Trace($"Attempting best match against {skills.Count} skills");



            


            return "Extracted";
        }
    }
}
