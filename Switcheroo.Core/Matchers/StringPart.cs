﻿namespace Switcheroo.Core.Matchers {
    public struct StringPart {
        public string Value { get; set; }
        public bool IsMatch { get; set; }

        public StringPart(string value, bool isMatch = false)
        {
            Value = value;
            IsMatch = isMatch;
        }
    }
}
