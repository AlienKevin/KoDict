using System.Text.RegularExpressions;
using Rules = System.Collections.Generic.List<Rule>;
record struct Rule(Regex pattern, string replacement, string reason)
{
    public Rule(string pattern, string replacement, string reason) : this(new Regex(pattern), replacement, reason) { }
}


class Hangeul2Sounds
{
    public struct Explanation
    {
        public string rule { get; }
        public string result { get; }

        public Explanation(string rule, string result)
        {
            this.rule = rule;
            this.result = result;
        }
    }

    private static (string, List<Explanation>) ApplyRules(Rules rules, string s)
    {
        string result = s;
        List<Explanation> appliedRules = new List<Explanation>();
        foreach ((Regex pattern, string template, string rule) in rules)
        {
            string newResult = pattern.Replace(result, template);
            if (result != newResult)
            {
                appliedRules.Add(new Explanation(rule, Hangeul.Jamos2Hangeul(newResult)));
                result = newResult;
            }
        }
        return (result, appliedRules);
    }

    private static Rules SyllableFinalNeutralization = new Rules {
        new Rule("(ᆿ|ᆩ)", "ᆨ", "ᆿ and ᆩ are neutralized to ᆨ at the end of a syllable"),
        new Rule("(ᇀ|ᆺ|ᆻ|ᆽ|ᆾ|ᇂ)", "ᆮ", "ᇀ, ᆺ, ᆻ, ᆽ, ᆾ, and ᇂ are neutralized to ᆮ at the end of a syllable"),
        new Rule("ᇁ", "ᆸ", "ᇁ is neutralized to ᆸ at the end of a syllable"),
        new Rule("ᆪ", "ᆨ", "ᆪ is neutralized to ᆨ at the end of a syllable"),
        new Rule("ᆬ", "ᆫ", "ᆬ is neutralized to ᆫ at the end of a syllable"),
        new Rule("ᆭ", "ᆫ", "ᆭ is neutralized to ᆫ at the end of a syllable"),
        new Rule("ᆰ", "ᆨ", "ᆰ is neutralized to ᆨ at the end of a syllable"),
        new Rule("ᆱ", "ᆷ", "ᆱ is neutralized to ᆷ at the end of a syllable"),
        new Rule("ᆲ", "ᆯ", "ᆲ is neutralized to ᆯ at the end of a syllable"),  // may also be neutralized to ᆸ in Seoul Korean
        new Rule("ᆳ", "ᆯ", "ᆳ is neutralized to ᆯ at the end of a syllable"),
        new Rule("ᆴ", "ᆯ", "ᆴ is neutralized to ᆯ at the end of a syllable"),
        new Rule("ᆵ", "ᆸ", "ᆵ is neutralized to ᆸ at the end of a syllable"),
        new Rule("ᆶ", "ᆯ", "ᆶ is neutralized to ᆯ at the end of a syllable"),
        new Rule("ᆹ", "ᆸ", "ᆹ is neutralized to ᆸ at the end of a syllable"),
    };

    private static Rules Liaison = new Rules {
        new Rule("ᆨᄋ", "ᄀ", "The final consonant ᆨ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆫᄋ", "ᄂ", "The final consonant ᆫ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆮᄋ", "ᄃ", "The final consonant ᆮ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆯᄋ", "ᄅ", "The final consonant ᆯ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆷᄋ", "ᄆ", "The final consonant ᆷ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆸᄋ", "ᄇ", "The final consonant ᆸ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆺᄋ", "ᄉ", "The final consonant ᆺ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆽᄋ", "ᄌ", "The final consonant ᆽ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆾᄋ", "ᄎ", "The final consonant ᆾ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆿᄋ", "ᄏ", "The final consonant ᆿ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᇀᄋ", "ᄐ", "The final consonant ᇀ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᇁᄋ", "ᄑ", "The final consonant ᇁ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᇂᄋ", "ᄒ", "The final consonant ᇂ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆪᄋ", "ᆨᄉ", "The final consonant ᆪ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆬᄋ", "ᆫᄌ", "The final consonant ᆬ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆭᄋ", "ᆫᄒ", "The final consonant ᆭ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆰᄋ", "ᆯᄀ", "The final consonant ᆰ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆱᄋ", "ᆯᄆ", "The final consonant ᆱ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆲᄋ", "ᆯᄇ", "The final consonant ᆲ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆳᄋ", "ᆯᄉ", "The final consonant ᆳ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆴᄋ", "ᆯᄐ", "The final consonant ᆴ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆵᄋ", "ᆸᄑ", "The final consonant ᆵ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆶᄋ", "ᆯᄒ", "The final consonant ᆶ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
        new Rule("ᆹᄋ", "ᆸᄉ", "The final consonant ᆹ shifts over to the beginning of the next syllable when there is a placeholder ᄋ"),
    };

    private static Rules Nasalization = new Rules {
        // Obstruent Nasalization
        new Rule("ᆸ(ᄆ|ᄂ)", "ᆷ${1}", "The obstruent ᆸ is nasalized to ᆷ before the nasals ᄆ and ᄂ"),
        new Rule("ᆮ(ᄆ|ᄂ)", "ᆫ${1}", "The obstruent ᆮ is nasalized to ᆫ before the nasals ᄆ and ᄂ"),
        new Rule("ᆨ(ᄆ|ᄂ)", "ᆼ${1}", "The obstruent ᆨ is nasalized to ᆼ before the nasals ᄆ and ᄂ"),
        new Rule("ᆸᄅ", "ᆷᄂ", "The obstruent ᆸ is nasalized to ᆷ before the liquid ᄅ and ᄅ is nasalized to ᄂ"),
        new Rule("ᆨᄅ", "ᆼᄂ", "The obstruent ᆨ is nasalized to ᆼ before the liquid ᄅ and ᄅ is nasalized to ᄂ"),
        // Liquid Nasalization
        // /m/ + /l/ → [m] + [n]
        // /ŋ/ + /l/ → [ŋ] + [n]
        new Rule("(ᆷ|ᆼ)ᄅ", "${1}ᄂ", "The liquid ᄅ is nasalized to ᄂ after the nasals ᆷ and ᆼ"),
    };

    private static Rules Palatalization = new Rules {
        new Rule("ᆮ이", "지", "The consonant ᆮ is palatalized to ᄌ before the vowel 이"),
        new Rule("ᆮ히", "치", "The consonant ᆮ is palatalized to ᄎ before the sound 히"),
        new Rule("ᇀ(히|이)", "치", "The consonant ᇀ is palatalized to ᄎ before the sounds 히 and 이"),
    };

    private static Rules Lateralization = new Rules {
        new Rule("ᆫᄅ|ᆯᄂ", "ᆯᄅ", "The nasal ᄂ is lateralized to the liquid ᆯ before or after the liquid ᆯ"),
    };

    private static Rules Aspiration = new Rules {
        new Rule("ᆸᄒ|ᇂᄇ", "ᄑ", "The obstruent ᆸ is aspirated to ᄑ before or after ᇂ"),
        new Rule("ᆮᄒ|ᇂᄃ", "ᄐ", "The obstruent ᆮ is aspirated to ᄐ before or after ᇂ"),
        new Rule("ᆨᄒ|ᇂᄀ", "ᄏ", "The obstruent ᆨ is aspirated to ᄏ before or after ᇂ"),
        new Rule("ᆽᄒ|ᇂᄌ", "ᄎ", "The obstruent ᆽ is aspirated to ᄎ before or after ᇂ"),

        new Rule("ᆭᄇ", "ᆫᄑ", "The plosive ᄇ is aspirated to ᄑ after the consonant cluster ᆭ"),
        new Rule("ᆭᄃ", "ᆫᄐ", "The plosive ᄃ is aspirated to ᄐ after the consonant cluster ᆭ"),
        new Rule("ᆭᄀ", "ᆫᄏ", "The plosive ᄀ is aspirated to ᄏ after the consonant cluster ᆭ"),

        new Rule("ᆶᄇ", "ᆯᄑ", "The plosive ᄇ is aspirated to ᄑ after the consonant cluster ᆶ"),
        new Rule("ᆶᄃ", "ᆯᄐ", "The plosive ᄃ is aspirated to ᄐ after the consonant cluster ᆶ"),
        new Rule("ᆶᄀ", "ᆯᄏ", "The plosive ᄀ is aspirated to ᄏ after the consonant cluster ᆶ"),

        new Rule("ᆰᄒ", "ᆯᄏ", "The plosive ᄀ in the consonant cluster ᆰ is aspirated to ᄏ before ᄒ"),
    };

    private static Rules Fortis = new Rules {
        new Rule("(ᆸ|ᆮ|ᆨ)ᄇ", "${1}ᄈ", "The obstruent ᄇ is tensified after the obstruents ᆸ, ᆮ, and ᆨ"),
        new Rule("(ᆸ|ᆮ|ᆨ)ᄃ", "${1}ᄄ", "The obstruent ᄃ is tensified after the obstruents ᆸ, ᆮ, and ᆨ"),
        new Rule("(ᆸ|ᆮ|ᆨ)ᄀ", "${1}ᄁ", "The obstruent ᄀ is tensified after the obstruents ᆸ, ᆮ, and ᆨ"),
        new Rule("(ᆸ|ᆮ|ᆨ)ᄉ", "${1}ᄊ", "The obstruent ᄉ is tensified after the obstruents ᆸ, ᆮ, and ᆨ"),
        new Rule("(ᆸ|ᆮ|ᆨ)ᄌ", "${1}ᄍ", "The obstruent ᄌ is tensified after the obstruents ᆸ, ᆮ, and ᆨ"),
        new Rule("ᆰᄀ", "ᆯᄁ", "The plosive ᄀ is tensified after the consonant cluster ᆰ"),
    };

    private static Rules SpecialCases = new Rules {
        new Rule("희", "히", "Special case: 희 is pronounced as 히"),
        new Rule("쳐", "처", "Special case: 쳐 is pronounced as 처"),
    };

    public static (string, List<Explanation>) ToSounds(string word)
    {
        // Remove whitespace between morphemes (only 5 words have whitespaces)
        word = word.Replace(" ", "");

        var surfacePhoneticRules = new List<Rules> {
            SpecialCases,
            Palatalization,
            Liaison,
            Lateralization,
            Aspiration,
            Fortis,
            SyllableFinalNeutralization,
            Aspiration,
            Fortis,
            Nasalization,
        };

        var (resultWord, appliedRules) = surfacePhoneticRules.Aggregate((Hangeul.Hangeuls2Jamos(word), new List<Explanation>()), (result, rules) =>
        {
            var (word, appliedRules) = result;
            var (resultWord, newAppliedRules) = ApplyRules(rules, word);
            return (resultWord, appliedRules.Concat(newAppliedRules).ToList());
        });

        return (Hangeul.Jamos2Hangeul(resultWord), appliedRules);
    }
}