using System;

public class IngredientFormatter
{
    public static class Convert
    {
        public const float k_toL = 0.001f;
        public const float k_toFlOz = 0.033814f;
        public const float k_toTsp = 0.202884f;
        public const float k_toTbsp = 0.067628f;
        public const float k_toCup = 0.00422675f;

        public const float k_toKg = 0.001f;
        public const float k_toOz = 0.035274f;
        public const float k_toLb = 0.00220462f;

        public const float k_fromL = 1000f;
        public const float k_fromFlOz = 29.5735f;
        public const float k_fromTsp = 4.92892f;
        public const float k_fromTbsp = 14.7868f;
        public const float k_fromCup = 236.588f;

        public const float k_fromKg = 1000f;
        public const float k_fromOz = 28.3495f;
        public const float k_fromLb = 453.592f;
    }

    public enum Fraction
    {
        Whole,
        Eighth,
        Quarter,
        Third,
        Half,
        TwoThird,
        ThreeQuarter
    }

    public Fraction RoundToFractionWEighth(ref float value)
    {
        float decimalPart = value - (int)value;
        value = (int)value;

        if (decimalPart < 0.0625f)
            return Fraction.Whole;
        if (decimalPart < 0.1875f)
            return Fraction.Eighth;
        if (decimalPart < 0.3f)
            return Fraction.Quarter;
        if (decimalPart < 0.4f)
            return Fraction.Third;
        if (decimalPart < 0.6f)
            return Fraction.Half;
        if (decimalPart < 0.7f)
            return Fraction.TwoThird;
        if (decimalPart < 0.875f)
            return Fraction.ThreeQuarter;

        value += 1.0f;
        return Fraction.Whole;
    }

    public Fraction RoundToFraction(ref float value)
    {
        float decimalPart = value - (int)value;
        value = (int)value;

        if (decimalPart < 0.125f)
            return Fraction.Whole;
        if (decimalPart < 0.3f)
            return Fraction.Quarter;
        if (decimalPart < 0.4f)
            return Fraction.Third;
        if (decimalPart < 0.6f)
            return Fraction.Half;
        if (decimalPart < 0.7f)
            return Fraction.TwoThird;
        if (decimalPart < 0.875f)
            return Fraction.ThreeQuarter;

        value += 1.0f;
        return Fraction.Whole;
    }

    public string FractionString(Fraction fraction, float whole)
    {
        string result = whole >= 1.0f ? ((int)whole).ToString() + (fraction != Fraction.Whole ? " " : "") : "";
        return fraction switch
        {
            Fraction.Eighth => result + "1/8",
            Fraction.Quarter => result + "1/4",
            Fraction.Half => result + "1/2",
            Fraction.ThreeQuarter => result + "3/4",
            Fraction.Third => result + "1/3",
            Fraction.TwoThird => result + "2/3",
            _ => result
        };
    }

    public string DecimalString(Fraction fraction, float whole)
    {
        string result = ((int)whole).ToString();
        return fraction switch
        {
            Fraction.Eighth => result + ".125",
            Fraction.Quarter => result + ".25",
            Fraction.Half => result + ".5",
            Fraction.ThreeQuarter => result + ".75",
            Fraction.Third => result + ".33",
            Fraction.TwoThird => result + ".66",
            _ => result
        };
    }

    public string ImperialMass(float amount, bool isFractional)
    {
        if (amount < Convert.k_fromLb)
        {
            amount *= Convert.k_toOz;
            amount = (float)Math.Round(amount);
            return ((int)amount).ToString() + " oz.";
        }

        amount *= Convert.k_toLb;
        Fraction frac = RoundToFraction(ref amount);
        return (isFractional ? FractionString(frac, amount) : DecimalString(frac, amount)) + " lb.";
    }

    public string ImperialVolume(float amount, bool preferOz, bool isFractional)
    {
        Fraction frac;
        if (preferOz)
        {
            amount *= Convert.k_toFlOz;
            frac = RoundToFraction(ref amount);
            return (isFractional ? FractionString(frac, amount) : DecimalString(frac, amount)) + " fl. oz.";
        }

        if (amount < Convert.k_fromTbsp)
        {
            float asTbsp = amount * Convert.k_toTbsp;
            frac = RoundToFraction(ref asTbsp);
            if (frac == Fraction.Half)
            {
                return (isFractional ? "1/2" : "0.5") + " tbsp.";
            }

            amount *= Convert.k_toTsp;
            frac = amount < 1 ? RoundToFractionWEighth(ref amount) : RoundToFraction(ref amount);
            return (isFractional ? FractionString(frac, amount) : DecimalString(frac, amount)) + " tsp.";
        }

        if (amount < Convert.k_fromCup * 0.25f)
        {
            amount *= Convert.k_toTbsp;
            frac = RoundToFraction(ref amount);
            return (isFractional ? FractionString(frac, amount) : DecimalString(frac, amount)) + " tbsp.";
        }

        amount *= Convert.k_toCup;
        frac = RoundToFraction(ref amount);
        string unit = amount > 1 ? " cups" : " cup";
        return (isFractional ? FractionString(frac, amount) : DecimalString(frac, amount)) + unit;
    }

    public string MetricMass(float amount)
    {
        amount = (float)Math.Round(amount);
        return amount < 1000.0f ? ((int)amount).ToString() + " g" : (amount * 0.001).ToString() + " kg";
    }

    public string MetricVolume(float amount)
    {
        amount = (float)Math.Round(amount);
        return amount < 1000.0f ? ((int)amount).ToString() + " mL" : (amount * 1000).ToString() + " L";
    }
}

