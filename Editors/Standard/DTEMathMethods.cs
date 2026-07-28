using System;
using System.Collections.Generic;
using System.Text;
using static OfficeOpenXml.ExcelErrorValue;

namespace GameEditorStudio
{
    public static class DTEMathMethods
    {

        public enum EntryValueSource //DO NOT RENAME, THESE TERMS SAVE TO XML. (If desperate, update the XML saving first).
        {
            FromCurrent,
            FromInput,
            FromOutput,
            
        }

        public static double GetEntryValueFrom(Entry entry, EntryValueSource source) 
        {
            if (source == EntryValueSource.FromCurrent) 
            {
                return GetEntryValueFromCurrent(entry);
            }
            if (source == EntryValueSource.FromInput)
            {
                return GetEntryValueFromInput(entry);
            }
            if (source == EntryValueSource.FromOutput)
            {
                PixelWPF.LibraryPixel.NotificationNegative("Entry Math Error Output", "I never coded getting value from output. add code for this :3");
                return 0;
            }           

            PixelWPF.LibraryPixel.NotificationNegative("Entry Math Error 1", "I returned a value of 0 to prevent crashing, but this is not correct. Probably don't save game files, and just restart GES.\n\nAlso please report this as a bug.");
            return 0;
        }


        public static double GetEntryValueFromCurrent(Entry entry) //Return the correct value of an entry, accounting for if it's signed or not.
        {
            double CurrentValue = double.Parse(entry.EntryByteDecimal);

            if (entry.NewSubType == Entry.EntrySubTypes.NumberBox)
            {
                if (entry.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed)
                {
                    double SignedValue = GetSignedValue(entry, CurrentValue);
                    return SignedValue;
                }
            }

            //ELSE            
            return CurrentValue;
        }

        public static double GetEntryValueFromInput(Entry entry) //Return the correct value of an entry, accounting for if it's signed or not.
        {
            double CurrentValue = double.Parse(entry.EntryValueOnProjectLoadFromInput);

            if (entry.NewSubType == Entry.EntrySubTypes.NumberBox)
            {
                if (entry.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed)
                {
                    double SignedValue = GetSignedValue(entry, CurrentValue);
                    return SignedValue;
                }
            }

            //ELSE            
            return CurrentValue;
        }

        public static double GetSignedValue(Entry entry, double CurrentValue)
        {
            if (entry.Bytes == 1)
            {
                if (CurrentValue > 127)
                {
                    return 127;
                }
                else if (CurrentValue < -128)
                {
                    return -128;
                }

            }
            if (entry.Bytes == 2)
            {
                if (CurrentValue > 32767)
                {
                    return 32767;
                }
                else if (CurrentValue < -32768)
                {
                    return -32768;
                }
            }
            if (entry.Bytes == 4)
            {
                if (CurrentValue > 2147483647)
                {
                    return 2147483647;
                }
                else if (CurrentValue < -2147483648)
                {
                    return -2147483648;
                }
            }
                        
            return CurrentValue;
        }








        public static double ReturnClampedValueForEntrySizeAndSign(Entry entry, double value)
        {
            //This method rounds the incoming value, then clamps it to the min and max possible values based on entry size and sign.

            value = Math.Round(value);

            if (entry.NewSubType == Entry.EntrySubTypes.NumberBox)
            {
                if (entry.EntryTypeNumberBox.NewNumberSign == EntryTypeNumberBox.TheNumberSigns.Signed)
                {
                    if (entry.Bytes == 1)
                    {
                        if (value > 127)
                        {
                            return 127;
                        }
                        else if (value < -128)
                        {
                            return -128;
                        }

                    }
                    if (entry.Bytes == 2)
                    {
                        if (value > 32767)
                        {
                            return 32767;
                        }
                        else if (value < -32768)
                        {
                            return -32768;
                        }
                    }
                    if (entry.Bytes == 4)
                    {
                        if (value > 2147483647)
                        {
                            return 2147483647;
                        }
                        else if (value < -2147483648)
                        {
                            return -2147483648;
                        }
                    }
                    return value;
                }
            }

            if (entry.Bytes == 1)
            {
                if (value > 255)
                {
                    return 255;
                }
                else if (value < 0)
                {
                    return 0;
                }

            }
            if (entry.Bytes == 2)
            {
                if (value > 65535)
                {
                    return 65535;
                }
                else if (value < 0)
                {
                    return 0;
                }
            }
            if (entry.Bytes == 4)
            {
                if (value > 4294967295)
                {
                    return 4294967295;
                }
                else
                {
                    return 0;
                }
            }

            
            return value;
        }

        
    }
}
