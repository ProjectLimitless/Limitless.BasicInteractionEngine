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
using System.Dynamic;
using System.Collections.Generic;

using Chronic;
using Humanizer;

using Limitless.Runtime.Enums;
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
        private readonly ILogger _log;
        /// <summary>
        /// The date/time parser.
        /// </summary>
        private readonly Parser _timeParser;

        /// <summary>
        /// Creates a new instance of the extractor using 
        /// the supplied <see cref="ILogger"/>.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> to use</param>
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
        public Actionable Extract(string input, Dictionary<string, Skill> skills)
        {
            input = input.ToLowerInvariant();
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
                int matchConfidence = intent.Actions.Count(input.Contains);
                matchConfidence += intent.Targets.Count(input.Contains);
                matchConfidence += skill.Locations.Count(input.Contains);
                
                if (matchConfidence <= 0 || bestMatchedSkill.Skill != null)
                {
                    if (matchConfidence > bestMatchedSkill.Confidence)
                    {
                        bestMatchedSkill.Skill = skill;
                        bestMatchedSkill.Confidence = matchConfidence;
                        bestMatchedSkill.Location = skill.Locations.First(input.Contains);
                    }
                    else if (matchConfidence == bestMatchedSkill.Confidence && matchConfidence > 0)
                    {
                        // TODO: Handle multiple matches better
                        throw new NotSupportedException("Multiple skills matched");
                    }
                }
                else
                {
                    bestMatchedSkill.Skill = skill;
                    bestMatchedSkill.Confidence = matchConfidence;
                    bestMatchedSkill.Location = skill.Locations.FirstOrDefault(input.Contains);
                }
            }
            
            if (bestMatchedSkill.Skill == null)
            {
                _log.Info("No skill matched");
                throw new InvalidOperationException("No skill matched");
            }
            _log.Debug($"Matched Skill '{bestMatchedSkill.Skill.Name}' with confidence {bestMatchedSkill.Confidence}");
            
            var actionable = new Actionable
            {
                Skill = bestMatchedSkill.Skill,
                Confidence = bestMatchedSkill.Confidence,
                Location = bestMatchedSkill.Location
            };

            if (actionable.Skill.Locations.Count > 0 && actionable.Location == null)
            {
                if (actionable.Skill.Locations.Count > 1)
                {
                    // Ask which one
                    actionable.AddQueryParameter(new SkillParameter("location", SkillParameterClass.Location));
                }
                else
                {
                    actionable.Location = actionable.Skill.Locations.First();
                }
            }
            
            var lookupParams = actionable.GetParametersByClass(SkillParameterClass.DateRange);
            if (lookupParams.Count > 0)
            {
                if (timeSpan != null)
                {
                    actionable.SkillParameters.Add(lookupParams.First().Parameter, dateRange);
                }
            }
            
            lookupParams = actionable.GetParametersByClass(SkillParameterClass.Quantity);
            actionable.SkillParameters =
                actionable.SkillParameters.Concat(ExtractParameters(input, lookupParams, "before")).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            
            lookupParams = actionable.GetParametersByClass(SkillParameterClass.IntegerValue);
            actionable.SkillParameters =
                actionable.SkillParameters.Concat(ExtractParameters(input, lookupParams, "after")).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


            return actionable;
        }

        /// <summary>
        /// Extract the parameters and their values from input.
        /// </summary>
        /// <param name="input">The input to extract from</param>
        /// <param name="parameters">The parameters to extract</param>
        /// <param name="matchDirection">The direction to search for in input</param> 
        /// <returns>The collection of extracted parameters and their values</returns>
        private Dictionary<string, object> ExtractParameters(string input, List<SkillParameter> parameters, string matchDirection)
        {
            var extractedParameters = new Dictionary<string, object>();
            foreach (SkillParameter parameter in parameters)
            {
                // Find the parameter in the input
                if (input.Contains(parameter.Parameter))
                {
                    // Then find the closest number before the parameter for quantity
                    double extractedValue = FindClosestQuantity(input, parameter.Parameter, matchDirection);
                    _log.Debug($"Extracted value of {extractedValue} for parameter '{parameter.Parameter}'");
                    extractedParameters.Add(parameter.Parameter, extractedValue);

                }
                else
                {
                    string checkParameter = parameter.Parameter;
                    // if parameter not found, check plural or singular form
                    if (checkParameter == checkParameter.Pluralize(false))
                    {
                        // Parameter is currently a plural, so singularize
                        checkParameter = checkParameter.Singularize();
                    }
                    else
                    {
                        // Parameter is currently singular, so pluralize
                        checkParameter.Pluralize();
                    }
                    _log.Trace($"Plural/Singular created of parameter '{parameter.Parameter}'. Matching '{checkParameter}'");

                    if (!input.Contains(parameter.Parameter))
                        continue;

                    // Then find the closest number before the parameter for quantity
                    double extractedValue = FindClosestQuantity(input, checkParameter, matchDirection);
                    _log.Debug($"Extracted value of {extractedValue} for parameter '{parameter.Parameter}'");
                    extractedParameters.Add(parameter.Parameter, extractedValue);
                }
            }
            return extractedParameters;
        }
        
        /// <summary>
        /// Finds the closest integer/double value to the given needle
        /// in haystack. The direction determines if the look should
        /// look only before, after or in any direction.
        /// </summary>
        /// <param name="haystack">The input to check for needle</param>
        /// <param name="needle">The word to search for</param>
        /// <param name="direction">The direction to search in</param>
        /// <returns>The value of the closest quantity</returns>
        private double FindClosestQuantity(string haystack, string needle, string direction)
        {
            double closestQuantity = 0.00;
            switch (direction)
            {
                case "before":
                    string before = haystack.Substring(0, haystack.IndexOf(needle, StringComparison.InvariantCultureIgnoreCase));
                    _log.Trace($"Matching for before direction: '{before}'");
                    foreach (string word in before.Split().Reverse())
                    {
                        if (double.TryParse(word, out closestQuantity))
                        {
                            break;
                        }
                    }
                    break;
                case "after":
                    string after = haystack.Substring(haystack.IndexOf(needle, StringComparison.InvariantCultureIgnoreCase));
                    _log.Trace($"Matching for after direction: '{after}'");
                    foreach (string word in after.Split())
                    {
                        if (double.TryParse(word, out closestQuantity))
                        {
                            break;
                        }
                    }
                    break;
                case "any":
                    throw new NotImplementedException();
                    break;
                default:
                    throw new NotImplementedException();
                    break;
            }
            return closestQuantity;
        }
    }
}
