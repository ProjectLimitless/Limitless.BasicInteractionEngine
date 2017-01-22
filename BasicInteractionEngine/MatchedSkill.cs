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

using Limitless.Runtime.Types;
using Limitless.Runtime.Interactions;

namespace Limitless.BasicInteractionEngine
{
    /// <summary>
    /// Holds information for a matched skill.
    /// </summary>
    public class MatchedSkill
    {
        /// <summary>
        /// Gets the matched skill.
        /// </summary>
        public Skill Skill { get; private set; }
        /// <summary>
        /// Gets the dates extracted from the input.
        /// </summary>
        public DateRange Dates { get; set; }
        /// <summary>
        /// Gets the confidence when matched.
        /// </summary>
        public int Confidence { get; private set; }

        /// <summary>
        /// Constructor setting the skill.
        /// </summary>
        /// <param name="skill">The matched skill</param>
        public MatchedSkill(Skill skill)
        {
            Skill = skill;
            Dates = new DateRange();
            Confidence = 0;
        }

        /// <summary>
        /// Constructor setting the matched skill and 
        /// extracted dates.
        /// </summary>
        /// <param name="skill">The matched skill</param>
        /// <param name="dates">The extracted dates</param>
        public MatchedSkill(Skill skill, DateRange dates)
        {
            Skill = skill;
            Dates = dates;
            Confidence = 0;
        }

        /// <summary>
        /// Constructor setting the matched skill,
        /// extracted dates and confidence.
        /// </summary>
        /// <param name="skill">The matched skill</param>
        /// <param name="dates">The extracted dates</param>
        /// <param name="confidence">The confidence of the match</param>
        public MatchedSkill(Skill skill, DateRange dates, int confidence)
        {
            Skill = skill;
            Dates = dates;
            Confidence = confidence;
        }
    }
}
