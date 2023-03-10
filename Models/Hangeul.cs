public class Hangeul
{

    public static int Jamo2HangeulCompatibilityJamo(int jamo)
    {
        if (jamo >= 0x1100 && jamo <= 0x11ff)
        {
            int result = hangeulCompatibilityJamos[jamo - 0x1100];
            if (result != 0x0000)
            {
                return result;
            }
            else
            {
                return jamo;
            }
        }
        else
        {
            return jamo;
        }
    }

    private static bool IsLPartJamo(int c)
    {
        return 0x1100 <= c && c <= 0x1112;
    }

    private static bool IsVPartJamo(int c)
    {
        return 0x1161 <= c && c <= 0x1175;
    }

    private static bool IsTPartJamo(int c)
    {
        return 0x11A8 <= c && c <= 0x11C2;
    }

    private static bool IsJamo(int c)
    {
        return IsLPartJamo(c) || IsVPartJamo(c) || IsTPartJamo(c);
    }

    public static string Hangeul2Jamos(char inp)
    {
        // Must only contain a single Hangeul character
        if (inp.ToString().Length > 1)
        {
            return inp.ToString();
        }

        int s = Convert.ToInt32(inp);
        int SIndex = s - SBase;
        if (SIndex < 0 || SIndex >= SCount)
        {
            return inp.ToString();
        }
        else
        {
            string result = "";
            int L = LBase + SIndex / NCount;
            int V = VBase + (SIndex % NCount) / TCount;
            int T = TBase + SIndex % TCount;
            result += char.ConvertFromUtf32(L);
            result += char.ConvertFromUtf32(V);
            if (T != TBase)
            {
                result += char.ConvertFromUtf32(T);
            }
            return result;
        }
    }

    public static string Jamos2Hangeul(string inp)
    {
        const int lState = 0;
        const int vState = 1;
        const int tState = 2;

        int partState = lState;
        int LVIndex = 0;

        string hangeul = "";

        string Part2String(int part)
        {
            return ((char)Jamo2HangeulCompatibilityJamo(part)).ToString();
        }

        foreach (var part in inp)
        {
            int partValue = (int)part;

            if (partState == lState)
            {
                if (IsLPartJamo(partValue))
                {
                    LVIndex = (partValue - LBase) * NCount;
                    partState = vState;
                }
                else
                {
                    hangeul += Part2String(partValue);
                }
            }
            else if (partState == vState)
            {
                if (IsVPartJamo(partValue))
                {
                    LVIndex += (partValue - VBase) * TCount;
                    partState = tState;
                }
                else
                {
                    int prevLPart = LVIndex / NCount + LBase;
                    if (IsLPartJamo(partValue))
                    {
                        hangeul += Part2String(prevLPart);
                        LVIndex = (partValue - LBase) * NCount;
                    }
                    else
                    {
                        hangeul += Part2String(prevLPart);
                        hangeul += Part2String(partValue);
                        partState = lState;
                    }
                }
            }
            else if (partState == tState)
            {
                int s = 0;
                string appendHangeul = "";

                if (IsTPartJamo(partValue))
                {
                    int TIndex = partValue - TBase;
                    s = SBase + LVIndex + TIndex;
                    partState = lState;
                }
                else if (IsLPartJamo(partValue))
                {
                    s = SBase + LVIndex;
                    LVIndex = (partValue - LBase) * NCount;
                    partState = vState;
                }
                else
                {
                    s = SBase + LVIndex;
                    appendHangeul = Part2String(partValue);
                    partState = lState;
                }

                hangeul += Part2String(s) + appendHangeul;
            }
        }

        if (partState == vState)
        {
            int prevLPart = LVIndex / NCount + LBase;
            hangeul += Part2String(prevLPart);
        }
        else if (partState == tState)
        {
            int s = SBase + LVIndex;
            hangeul += Part2String(s);
        }

        return hangeul;
    }

    public static string Hangeuls2Jamos(string inp)
    {
        string jamos = "";
        foreach (char hangeul in inp)
        {
            jamos += Hangeul2Jamos(hangeul);
        }
        return jamos;
    }

    // Hangeul Composition/Decomposition constants
    private static int SBase = 0xAC00;

    private static int LBase = 0x1100;
    private static int VBase = 0x1161;
    private static int TBase = 0x11A7;
    private static int LCount = 19;
    private static int TCount = 28;
    private static int NCount = 588; // VCount * TCount
    private static int SCount = LCount * NCount;   // 11172

    private static int[] hangeulCompatibilityJamos = new int[] {
    0x3131,     /* 0x1100 */
    0x3132,     /* 0x1101 */
    0x3134,     /* 0x1102 */
    0x3137,     /* 0x1103 */
    0x3138,     /* 0x1104 */
    0x3139,     /* 0x1105 */
    0x3141,     /* 0x1106 */
    0x3142,     /* 0x1107 */
    0x3143,     /* 0x1108 */
    0x3145,     /* 0x1109 */
    0x3146,     /* 0x110a */
    0x3147,     /* 0x110b */
    0x3148,     /* 0x110c */
    0x3149,     /* 0x110d */
    0x314a,     /* 0x110e */
    0x314b,     /* 0x110f */
    0x314c,     /* 0x1110 */
    0x314d,     /* 0x1111 */
    0x314e,     /* 0x1112 */
    0x0000,     /* 0x1113 */
    0x3165,     /* 0x1114 */
    0x3166,     /* 0x1115 */
    0x0000,     /* 0x1116 */
    0x0000,     /* 0x1117 */
    0x0000,     /* 0x1118 */
    0x0000,     /* 0x1119 */
    0x3140,     /* 0x111a */
    0x0000,     /* 0x111b */
    0x316e,     /* 0x111c */
    0x3171,     /* 0x111d */
    0x3172,     /* 0x111e */
    0x0000,     /* 0x111f */
    0x3173,     /* 0x1120 */
    0x3144,     /* 0x1121 */
    0x3174,     /* 0x1122 */
    0x3175,     /* 0x1123 */
    0x0000,     /* 0x1124 */
    0x0000,     /* 0x1125 */
    0x0000,     /* 0x1126 */
    0x3176,     /* 0x1127 */
    0x0000,     /* 0x1128 */
    0x3177,     /* 0x1129 */
    0x0000,     /* 0x112a */
    0x3178,     /* 0x112b */
    0x3179,     /* 0x112c */
    0x317a,     /* 0x112d */
    0x317b,     /* 0x112e */
    0x317c,     /* 0x112f */
    0x0000,     /* 0x1130 */
    0x0000,     /* 0x1131 */
    0x317d,     /* 0x1132 */
    0x0000,     /* 0x1133 */
    0x0000,     /* 0x1134 */
    0x0000,     /* 0x1135 */
    0x317e,     /* 0x1136 */
    0x0000,     /* 0x1137 */
    0x0000,     /* 0x1138 */
    0x0000,     /* 0x1139 */
    0x0000,     /* 0x113a */
    0x0000,     /* 0x113b */
    0x0000,     /* 0x113c */
    0x0000,     /* 0x113d */
    0x0000,     /* 0x113e */
    0x0000,     /* 0x113f */
    0x317f,     /* 0x1140 */
    0x0000,     /* 0x1141 */
    0x0000,     /* 0x1142 */
    0x0000,     /* 0x1143 */
    0x0000,     /* 0x1144 */
    0x0000,     /* 0x1145 */
    0x0000,     /* 0x1146 */
    0x3180,     /* 0x1147 */
    0x0000,     /* 0x1148 */
    0x0000,     /* 0x1149 */
    0x0000,     /* 0x114a */
    0x0000,     /* 0x114b */
    0x3181,     /* 0x114c */
    0x0000,     /* 0x114d */
    0x0000,     /* 0x114e */
    0x0000,     /* 0x114f */
    0x0000,     /* 0x1150 */
    0x0000,     /* 0x1151 */
    0x0000,     /* 0x1152 */
    0x0000,     /* 0x1153 */
    0x0000,     /* 0x1154 */
    0x0000,     /* 0x1155 */
    0x0000,     /* 0x1156 */
    0x3184,     /* 0x1157 */
    0x3185,     /* 0x1158 */
    0x3186,     /* 0x1159 */
    0x0000,     /* 0x115a */
    0x0000,     /* 0x115b */
    0x0000,     /* 0x115c */
    0x0000,     /* 0x115d */
    0x0000,     /* 0x115e */
    0x0000,     /* 0x115f */
    0x3164,     /* 0x1160 */
    0x314f,     /* 0x1161 */
    0x3150,     /* 0x1162 */
    0x3151,     /* 0x1163 */
    0x3152,     /* 0x1164 */
    0x3153,     /* 0x1165 */
    0x3154,     /* 0x1166 */
    0x3155,     /* 0x1167 */
    0x3156,     /* 0x1168 */
    0x3157,     /* 0x1169 */
    0x3158,     /* 0x116a */
    0x3159,     /* 0x116b */
    0x315a,     /* 0x116c */
    0x315b,     /* 0x116d */
    0x315c,     /* 0x116e */
    0x315d,     /* 0x116f */
    0x315e,     /* 0x1170 */
    0x315f,     /* 0x1171 */
    0x3160,     /* 0x1172 */
    0x3161,     /* 0x1173 */
    0x3162,     /* 0x1174 */
    0x3163,     /* 0x1175 */
    0x0000,     /* 0x1176 */
    0x0000,     /* 0x1177 */
    0x0000,     /* 0x1178 */
    0x0000,     /* 0x1179 */
    0x0000,     /* 0x117a */
    0x0000,     /* 0x117b */
    0x0000,     /* 0x117c */
    0x0000,     /* 0x117d */
    0x0000,     /* 0x117e */
    0x0000,     /* 0x117f */
    0x0000,     /* 0x1180 */
    0x0000,     /* 0x1181 */
    0x0000,     /* 0x1182 */
    0x0000,     /* 0x1183 */
    0x3187,     /* 0x1184 */
    0x3188,     /* 0x1185 */
    0x0000,     /* 0x1186 */
    0x0000,     /* 0x1187 */
    0x3189,     /* 0x1188 */
    0x0000,     /* 0x1189 */
    0x0000,     /* 0x118a */
    0x0000,     /* 0x118b */
    0x0000,     /* 0x118c */
    0x0000,     /* 0x118d */
    0x0000,     /* 0x118e */
    0x0000,     /* 0x118f */
    0x0000,     /* 0x1190 */
    0x318a,     /* 0x1191 */
    0x318b,     /* 0x1192 */
    0x0000,     /* 0x1193 */
    0x318c,     /* 0x1194 */
    0x0000,     /* 0x1195 */
    0x0000,     /* 0x1196 */
    0x0000,     /* 0x1197 */
    0x0000,     /* 0x1198 */
    0x0000,     /* 0x1199 */
    0x0000,     /* 0x119a */
    0x0000,     /* 0x119b */
    0x0000,     /* 0x119c */
    0x0000,     /* 0x119d */
    0x318d,     /* 0x119e */
    0x0000,     /* 0x119f */
    0x0000,     /* 0x11a0 */
    0x318e,     /* 0x11a1 */
    0x0000,     /* 0x11a2 */
    0x0000,     /* 0x11a3 */
    0x0000,     /* 0x11a4 */
    0x0000,     /* 0x11a5 */
    0x0000,     /* 0x11a6 */
    0x0000,     /* 0x11a7 */
    0x3131,     /* 0x11a8 */
    0x3132,     /* 0x11a9 */
    0x3133,     /* 0x11aa */
    0x3134,     /* 0x11ab */
    0x3135,     /* 0x11ac */
    0x3136,     /* 0x11ad */
    0x3137,     /* 0x11ae */
    0x3139,     /* 0x11af */
    0x313a,     /* 0x11b0 */
    0x313b,     /* 0x11b1 */
    0x313c,     /* 0x11b2 */
    0x313d,     /* 0x11b3 */
    0x313e,     /* 0x11b4 */
    0x313f,     /* 0x11b5 */
    0x3140,     /* 0x11b6 */
    0x3141,     /* 0x11b7 */
    0x3142,     /* 0x11b8 */
    0x3144,     /* 0x11b9 */
    0x3145,     /* 0x11ba */
    0x3146,     /* 0x11bb */
    0x3147,     /* 0x11bc */
    0x3148,     /* 0x11bd */
    0x314a,     /* 0x11be */
    0x314b,     /* 0x11bf */
    0x314c,     /* 0x11c0 */
    0x314d,     /* 0x11c1 */
    0x314e,     /* 0x11c2 */
    0x0000,     /* 0x11c3 */
    0x0000,     /* 0x11c4 */
    0x0000,     /* 0x11c5 */
    0x0000,     /* 0x11c6 */
    0x3167,     /* 0x11c7 */
    0x3168,     /* 0x11c8 */
    0x0000,     /* 0x11c9 */
    0x0000,     /* 0x11ca */
    0x0000,     /* 0x11cb */
    0x3169,     /* 0x11cc */
    0x0000,     /* 0x11cd */
    0x316a,     /* 0x11ce */
    0x0000,     /* 0x11cf */
    0x0000,     /* 0x11d0 */
    0x0000,     /* 0x11d1 */
    0x0000,     /* 0x11d2 */
    0x316b,     /* 0x11d3 */
    0x0000,     /* 0x11d4 */
    0x0000,     /* 0x11d5 */
    0x0000,     /* 0x11d6 */
    0x316c,     /* 0x11d7 */
    0x0000,     /* 0x11d8 */
    0x316d,     /* 0x11d9 */
    0x0000,     /* 0x11da */
    0x0000,     /* 0x11db */
    0x0000,     /* 0x11dc */
    0x316f,     /* 0x11dd */
    0x0000,     /* 0x11de */
    0x3170,     /* 0x11df */
    0x0000,     /* 0x11e0 */
    0x0000,     /* 0x11e1 */
    0x0000,     /* 0x11e2 */
    0x0000,     /* 0x11e3 */
    0x0000,     /* 0x11e4 */
    0x0000,     /* 0x11e5 */
    0x0000,     /* 0x11e6 */
    0x0000,     /* 0x11e7 */
    0x0000,     /* 0x11e8 */
    0x0000,     /* 0x11e9 */
    0x0000,     /* 0x11ea */
    0x0000,     /* 0x11eb */
    0x0000,     /* 0x11ec */
    0x0000,     /* 0x11ed */
    0x0000,     /* 0x11ee */
    0x0000,     /* 0x11ef */
    0x0000,     /* 0x11f0 */
    0x3182,     /* 0x11f1 */
    0x3183,     /* 0x11f2 */
    0x0000,     /* 0x11f3 */
    0x0000,     /* 0x11f4 */
    0x0000,     /* 0x11f5 */
    0x0000,     /* 0x11f6 */
    0x0000,     /* 0x11f7 */
    0x0000,     /* 0x11f8 */
    0x0000,     /* 0x11f9 */
    0x0000,     /* 0x11fa */
    0x0000,     /* 0x11fb */
    0x0000,     /* 0x11fc */
    0x0000,     /* 0x11fd */
    0x0000,     /* 0x11fe */
    0x0000,     /* 0x11ff */
    };

}