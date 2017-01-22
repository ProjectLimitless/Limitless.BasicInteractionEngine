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
using System.Dynamic;
using System.Collections.Generic;

using Chronic;

using Limitless.Runtime.Types;
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
        /// <returns>The matched skill with metadata</returns>
        public MatchedSkill Extract(string input, Dictionary<string, Skill> skills)
        {
            _log.Trace($"Extracting intent from '{input}'");

            // Extract the dates.
            var dateRange = new DateRange();
            Span timeSpan = _timeParser.Parse(input);
            if (timeSpan != null)
            {
                if (timeSpan.Start.HasValue)
                {
                    dateRange.Start = timeSpan.Start.Value;
                }
                if (timeSpan.End.HasValue)
                {
                    dateRange.End = timeSpan.End.Value;
                }
                _log.Trace($"Parser: {timeSpan.Start}");
                _log.Trace($"Parser: {timeSpan.End}");
                _log.Trace($"Parser: {timeSpan.Width}");
            }

            _log.Trace($"Attempting best match against {skills.Count} skills");

            // Very (very) basic intent matching. Go through all the intents
            // and add a confidence point for everytime an action or target
            // is found in the iput sentence.
            // TODO: Use matchedskill
            dynamic bestMatchedSkill = new ExpandoObject();
            bestMatchedSkill.Confidence = 0;
            bestMatchedSkill.Skill = null;
            foreach (KeyValuePair<string, Skill> kvp in skills)
            {
                var skill = kvp.Value;
                var intent = skill.Intent;
                int matchConfidence = 0;

                foreach (string action in intent.Actions)
                {
                    if (input.Contains(action))
                    {
                        matchConfidence++;
                    }
                }
                foreach (string target in intent.Targets)
                {
                    if (input.Contains(target))
                    {
                        matchConfidence++;
                    }
                }

                if (matchConfidence > 0 && bestMatchedSkill.Skill == null)
                {
                    bestMatchedSkill.Skill = skill;
                    bestMatchedSkill.Confidence = matchConfidence;
                }
                else if (matchConfidence > bestMatchedSkill.Confidence)
                {
                    bestMatchedSkill.Skill = skill;
                    bestMatchedSkill.Confidence = matchConfidence;
                }
                else if (matchConfidence == bestMatchedSkill.Confidence)
                {
                    throw new NotSupportedException("Multiple skills matched");
                }
            }
            
            if (bestMatchedSkill.Skill != null)
            {
                _log.Debug($"Matched Skill '{bestMatchedSkill.Skill.Name}' with confidence {bestMatchedSkill.Confidence}");
            }
            else
            {
                _log.Info("No skill matched");
                throw new InvalidOperationException("No skill matched");
            }
            
            return new MatchedSkill(bestMatchedSkill.Skill, dateRange, bestMatchedSkill.Confidence);
        }
    }
}
