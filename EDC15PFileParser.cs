// Todo: find cowFUN_AGR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.NetworkInformation;
using System.Net;
using DevExpress.Internal;
using System.Drawing;

namespace VAGSuite
{
    public class EDC15PFileParser : IEDCFileParser
    {

        public override string ExtractInfo(byte[] allBytes)
        {
            // assume info will be @ 0x53452 12 bytes
            string retval = string.Empty;
            try
            {
                int partnumberAddress = Tools.Instance.findSequence(allBytes, 0, new byte[5] { 0x45, 0x44, 0x43, 0x20, 0x20 }, new byte[5] { 1, 1, 1, 1, 1 });
                if (partnumberAddress > 0)
                {
                    retval = System.Text.ASCIIEncoding.ASCII.GetString(allBytes, partnumberAddress - 8, 12).Trim();
                }
            }
            catch (Exception)
            {
            }
            return retval;
        }

        public override string ExtractPartnumber(byte[] allBytes)
        {
            // assume info will be @ 0x53446 12 bytes
            string retval = string.Empty;
            try
            {
                int partnumberAddress = Tools.Instance.findSequence(allBytes, 0, new byte[5] { 0x45, 0x44, 0x43, 0x20, 0x20 }, new byte[5] { 1, 1, 1, 1, 1 });
                if (partnumberAddress > 0)
                {
                    retval = System.Text.ASCIIEncoding.ASCII.GetString(allBytes, partnumberAddress - 20, 12).Trim();
                }
            }
            catch (Exception)
            {
            }
            return retval;
        }

        public override string ExtractSoftwareNumber(byte[] allBytes)
        {
            string retval = string.Empty;
            try
            {
                int partnumberAddress = Tools.Instance.findSequence(allBytes, 0, new byte[5] { 0x45, 0x44, 0x43, 0x20, 0x20 }, new byte[5] { 1, 1, 1, 1, 1 });
                if (partnumberAddress > 0)
                {
                    retval = System.Text.ASCIIEncoding.ASCII.GetString(allBytes, partnumberAddress + 5, 8).Trim();
                    retval = retval.Replace(" ", "");
                }
            }
            catch (Exception)
            {
            }
            return retval;
        }

        public override string ExtractBoschPartnumber(byte[] allBytes)
        {
            return Tools.Instance.ExtractBoschPartnumber(allBytes);
            string retval = string.Empty;
            try
            {
                int partnumberAddress = Tools.Instance.findSequence(allBytes, 0, new byte[5] { 0x45, 0x44, 0x43, 0x20, 0x20 }, new byte[5] { 1, 1, 1, 1, 1 });
                if (partnumberAddress > 0)
                {
                    retval = System.Text.ASCIIEncoding.ASCII.GetString(allBytes, partnumberAddress + 23, 10).Trim();
                }
            }
            catch (Exception)
            {
            }
            return retval;
        }

        private string DetermineNumberByFlashBank(long address, List<CodeBlock> currBlocks)
        {
            foreach (CodeBlock cb in currBlocks)
            {
                if (cb.StartAddress <= address && cb.EndAddress >= address)
                {
                  //  if (cb.CodeID == 1) return "codeblock 1";// - MAN";
                  //  if (cb.CodeID == 2) return "codeblock 2";// - AUT (hydr)";
                  //  if (cb.CodeID == 3) return "codeblock 3";// - AUT (elek)";
                  //  return cb.CodeID.ToString();
                    if (cb.BlockGearboxType == GearboxType.Automatic)
                    {
                        return "codeblock " + cb.CodeID.ToString() + ", automatic";
                    }
                    else if (cb.CodeID == 2) return "codeblock " + cb.CodeID.ToString() + ", manual";
                    else if (cb.CodeID == 3) return "codeblock " + cb.CodeID.ToString() + ", 4x4";
                    return "codeblock " + cb.CodeID.ToString();
                }
            }
            long bankNumber = address / 0x10000;
            return "flashbank " + bankNumber.ToString();
        }

        private int DetermineCodeBlockByByAddress(long address, List<CodeBlock> currBlocks)
        {
            foreach (CodeBlock cb in currBlocks)
            {
                if (cb.StartAddress <= address && cb.EndAddress >= address)
                {
                    return cb.CodeID;
                }
            }
            return 0;
        }


        public override SymbolCollection parseFile(string filename, out List<CodeBlock> newCodeBlocks, out List<AxisHelper> newAxisHelpers)
        {
            newCodeBlocks = new List<CodeBlock>();
            SymbolCollection newSymbols = new SymbolCollection();
            newAxisHelpers = new List<AxisHelper>();
            byte[] allBytes = File.ReadAllBytes(filename);
            string boschnumber = ExtractBoschPartnumber(allBytes);
            string softwareNumber = ExtractSoftwareNumber(allBytes);
            partNumberConverter pnc = new partNumberConverter();

            VerifyCodeBlocks(allBytes, newSymbols, newCodeBlocks);

            for (int t = 0; t < allBytes.Length - 1; t += 2)
            {
                int len2skip = 0;
                //if (t == 0x4dc26) Console.WriteLine("ho");
                if (CheckMap(t, allBytes, newSymbols, newCodeBlocks, out len2skip))
                {
                    int from = t;
                    if (len2skip > 2) len2skip -= 2; // make sure we don't miss maps
                    if ((len2skip % 2) > 0) len2skip -= 1;
                    if (len2skip < 0) len2skip = 0;
                    t += len2skip;
                    /*                    if (from > 0x4dc00 && from < 0x4dd00)
                                        {
                                           // Console.WriteLine("map detected: " + from.ToString("X8") + " - " + t.ToString("X8") + " len: " + len2skip.ToString("X8"));
                                        }*/
                }
            }

            newSymbols.SortColumn = "Flash_start_address";
            newSymbols.SortingOrder = GenericComparer.SortOrder.Ascending;
            newSymbols.Sort();
            NameKnownMaps(allBytes, newSymbols, newCodeBlocks);

            BuildAxisIDList(newSymbols, newAxisHelpers);
            MatchAxis(newSymbols, newAxisHelpers);

            RemoveNonSymbols(newSymbols, newCodeBlocks);
            FindSVBL(allBytes, filename, newSymbols, newCodeBlocks);
            FindVCDSDiagLimits(allBytes, filename, newSymbols, newCodeBlocks);
            FindMiscMaps(allBytes, filename, newSymbols, newCodeBlocks);
            SymbolTranslator strans = new SymbolTranslator();
            foreach (SymbolHelper sh in newSymbols)
            {
                sh.Description = strans.TranslateSymbolToHelpText(sh.Varname);
            }
            // check for must have maps... if there are maps missing, report it
            return newSymbols;
        }

        private void MatchAxis(SymbolCollection newSymbols, List<AxisHelper> newAxisHelpers)
        {
            foreach (SymbolHelper sh in newSymbols)
            {
                if (!sh.YaxisAssigned)
                {
                    foreach (AxisHelper ah in newAxisHelpers)
                    {
                        if (sh.X_axis_ID == ah.AxisID)
                        {
                            sh.Y_axis_descr = ah.Description;
                            sh.YaxisUnits = ah.Units;
                            sh.Y_axis_offset = ah.Offset;
                            sh.Y_axis_correction = ah.Correction;
                            break;
                        }
                    }
                }
                if (!sh.XaxisAssigned)
                {
                    foreach (AxisHelper ah in newAxisHelpers)
                    {
                        if (sh.Y_axis_ID == ah.AxisID)
                        {
                            sh.X_axis_descr = ah.Description;
                            sh.XaxisUnits = ah.Units;
                            sh.X_axis_offset = ah.Offset;
                            sh.X_axis_correction = ah.Correction;
                            break;
                        }
                    }
                }

            }
        }

        private void BuildAxisIDList(SymbolCollection newSymbols, List<AxisHelper> newAxisHelpers)
        {
            foreach (SymbolHelper sh in newSymbols)
            {
                if (!sh.Varname.StartsWith("2D") && !sh.Varname.StartsWith("3D"))
                {
                    AddToAxisCollection(newAxisHelpers, sh.Y_axis_ID, sh.X_axis_descr, sh.XaxisUnits, sh.X_axis_correction, sh.X_axis_offset);
                    AddToAxisCollection(newAxisHelpers, sh.X_axis_ID, sh.Y_axis_descr, sh.YaxisUnits, sh.Y_axis_correction, sh.Y_axis_offset);
                }
            }
        }

        private void AddToAxisCollection(List<AxisHelper> newAxisHelpers, int ID, string descr, string units, double correction, double offset)
        {
            if (ID == 0) return;
            foreach (AxisHelper ah in newAxisHelpers)
            {
                if (ah.AxisID == ID) return;
            }
            AxisHelper ahnew = new AxisHelper();
            ahnew.AxisID = ID;
            ahnew.Description = descr;
            ahnew.Units = units;
            ahnew.Correction = correction;
            ahnew.Offset = offset;
            newAxisHelpers.Add(ahnew);
        }

        private void RemoveNonSymbols(SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            if (newCodeBlocks.Count > 0)
            {
                foreach (SymbolHelper sh in newSymbols)
                {
                    if (sh.CodeBlock == 0 && (sh.Varname.StartsWith("2D") || sh.Varname.StartsWith("3D")))
                    {
                        sh.Subcategory = "Zero codeblock stuff";

                    }
                }
            }
        }

        public override void FindSVBL(byte[] allBytes, string filename, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            if (!FindSVBLSequenceOne(allBytes, filename, newSymbols, newCodeBlocks))
            {
                FindSVBLSequenceTwo(allBytes, filename, newSymbols, newCodeBlocks);
            }
        }

        public void FindVCDSDiagLimits(byte[] allBytes, string filename, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            FindVCDSIQDiag1(allBytes, filename, newSymbols, newCodeBlocks);
        }

        public void FindMiscMaps(byte[] allBytes, string filename, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            FindSVRLSequence(allBytes, filename, newSymbols, newCodeBlocks);
            //FindPIDmaps(allBytes, filename, newSymbols, newCodeBlocks);
            FindBIPline(allBytes, filename, newSymbols, newCodeBlocks);
            //Findmrwmaps1(allBytes, filename, newSymbols, newCodeBlocks);
            Findmrwmaps2(allBytes, filename, newSymbols, newCodeBlocks);
        }

        private bool FindSVBLSequenceTwo(byte[] allBytes, string filename, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            bool found = true;
            bool SVBLFound = false;
            int offset = 0;
            while (found)
            {

                int SVBLAddress = Tools.Instance.findSequence(allBytes, offset, new byte[10] { 0xDF, 0x7A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0xDF, 0x7A }, new byte[10] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
                if (SVBLAddress > 0)
                {
                    SVBLFound = true;
                    SymbolHelper shsvbl = new SymbolHelper();
                    shsvbl.Category = "Detected maps";
                    shsvbl.Subcategory = "Limiters";
                    shsvbl.Flash_start_address = SVBLAddress - 2;
                    //shsvbl.Flash_start_address = SVBLAddress + 16;

                    // if value = 0xC3 0x00 -> two more back
                    int[] testValue = Tools.Instance.readdatafromfileasint(filename, (int)shsvbl.Flash_start_address, 1, EDCFileType.EDC15P);
                    if (testValue[0] == 0xC300) shsvbl.Flash_start_address -= 2;

                    shsvbl.Varname = "SVBL Boost limiter [" + DetermineNumberByFlashBank(shsvbl.Flash_start_address, newCodeBlocks) + "]";
                    shsvbl.Length = 2;
                    shsvbl.CodeBlock = DetermineCodeBlockByByAddress(shsvbl.Flash_start_address, newCodeBlocks);
                    newSymbols.Add(shsvbl);

                    //int MAPMAFSwitch = Tools.Instance.findSequence(allBytes, SVBLAddress - 0x100, new byte[8] { 0x41, 0x02, 0xFF, 0xFF, 0x00, 0x01, 0x01, 0x00 }, new byte[8] { 1, 1, 0, 0, 1, 1, 1, 1 });
                    int MAPMAFSwitch = Tools.Instance.findSequence(allBytes, SVBLAddress - 0x100, new byte[8] { 0x41, 0x02, 0xFF, 0xFF, 0x00, 0x01, 0x01, 0x00 }, new byte[8] { 1, 1, 0, 0, 1, 1, 1, 1 });
                    if (MAPMAFSwitch > 0)
                    {
                        MAPMAFSwitch += 2;
                        SymbolHelper mapmafsh = new SymbolHelper();
                        //mapmafsh.BitMask = 0x0101;
                        mapmafsh.Category = "Detected maps";
                        mapmafsh.Subcategory = "Switches";
                        mapmafsh.Flash_start_address = MAPMAFSwitch;
                        mapmafsh.Varname = "MAP/MAF switch (0 = MAF, 257/0x101 = MAP)" + DetermineNumberByFlashBank(shsvbl.Flash_start_address, newCodeBlocks);
                        mapmafsh.Length = 2;
                        mapmafsh.CodeBlock = DetermineCodeBlockByByAddress(mapmafsh.Flash_start_address, newCodeBlocks);
                        newSymbols.Add(mapmafsh);
                       //Console.WriteLine("Found MAP MAF switch @ " + MAPMAFSwitch.ToString("X8"));
                    }


                    offset = SVBLAddress + 1;
                }
                else found = false;
            }
            return SVBLFound;
        }

        private bool FindSVBLSequenceOne(byte[] allBytes, string filename, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            bool found = true;
            bool SVBLFound = false;
            int offset = 0;
            int offset2 = 0;
            while (found)
            {
                //int SVBLAddress = Tools.Instance.findSequence(allBytes, offset, new byte[10] { 0xDF, 0x7A, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0xDF, 0x7A }, new byte[10] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
                int SVBLAddress = Tools.Instance.findSequence(allBytes, offset, new byte[16] { 0xD2, 0x00, 0xFC, 0x03, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0xFF, 0xFF, 0xFF, 0xC3, 0x00, 0x00 }, new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1 });

                if (SVBLAddress > 0)
                {
                    //Console.WriteLine("Alternative SVBL " + SVBLAddress.ToString("X8"));
                    SVBLFound = true;
                    SymbolHelper shsvbl = new SymbolHelper();
                    shsvbl.Category = "Detected maps";
                    shsvbl.Subcategory = "Limiters";
                    //shsvbl.Flash_start_address = SVBLAddress - 2;
                    shsvbl.Flash_start_address = SVBLAddress + 16;

                    // if value = 0xC3 0x00 -> two more back
                    int[] testValue = Tools.Instance.readdatafromfileasint(filename, (int)shsvbl.Flash_start_address, 1, EDCFileType.EDC15P);
                    if (testValue[0] == 0xC300) shsvbl.Flash_start_address -= 2;

                    shsvbl.Varname = "SVBL Boost limiter [" + DetermineNumberByFlashBank(shsvbl.Flash_start_address, newCodeBlocks) + "]";
                    shsvbl.Length = 2;
                    shsvbl.CodeBlock = DetermineCodeBlockByByAddress(shsvbl.Flash_start_address, newCodeBlocks);
                    newSymbols.Add(shsvbl);

                    //int MAPMAFSwitch = Tools.Instance.findSequence(allBytes, SVBLAddress - 0x100, new byte[8] { 0x41, 0x02, 0xFF, 0xFF, 0x00, 0x01, 0x01, 0x00 }, new byte[8] { 1, 1, 0, 0, 1, 1, 1, 1 });
                    int MAPMAFSwitch = Tools.Instance.findSequence(allBytes, offset2, new byte[165] { 0xC3, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x02, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xC3, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xC3 }, new byte[165] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 });
                    if (MAPMAFSwitch > 0)
                    {
                        //MAPMAFSwitch += 2;
                        SymbolHelper mapmafsh = new SymbolHelper();
                        //mapmafsh.BitMask = 0x0101;
                        mapmafsh.Category = "Detected maps";
                        mapmafsh.Subcategory = "Switches";
                        mapmafsh.Flash_start_address = MAPMAFSwitch + 83;
                        mapmafsh.Varname = "MAP/MAF switch (cowBEG_BOO/cowBEG_P_L) [" + DetermineNumberByFlashBank(shsvbl.Flash_start_address, newCodeBlocks) + "]";
                        mapmafsh.Length = 2;
                        mapmafsh.Z_axis_descr = "(0 = MAF, 257 = Intake Boost pressure, 1 = Intake Atm Pressure)";
                        mapmafsh.CodeBlock = DetermineCodeBlockByByAddress(mapmafsh.Flash_start_address, newCodeBlocks);
                        newSymbols.Add(mapmafsh);
                        //Console.WriteLine("Found MAP MAF switch @ " + MAPMAFSwitch.ToString("X8"));
                        offset2 = MAPMAFSwitch + 1;
                    }
                    else
                    {
                        MAPMAFSwitch = Tools.Instance.findSequence(allBytes, offset2, new byte[8] { 0x41, 0x02, 0xFF, 0xFF, 0x00, 0x01, 0x01, 0x00 }, new byte[8] { 1, 1, 0, 0, 1, 1, 1, 1 }); 
                        if (MAPMAFSwitch > 0)
                        {
                            //MAPMAFSwitch += 2;
                            SymbolHelper mapmafsh = new SymbolHelper();
                            //mapmafsh.BitMask = 0x0101;
                            mapmafsh.Category = "Detected maps";
                            mapmafsh.Subcategory = "Switches";
                            mapmafsh.Flash_start_address = MAPMAFSwitch + 2;
                            mapmafsh.Varname = "MAP/MAF switch (cowBEG_BOO/cowBEG_P_L) [" + DetermineNumberByFlashBank(shsvbl.Flash_start_address, newCodeBlocks) + "]";
                            mapmafsh.Length = 2;
                            mapmafsh.Z_axis_descr = "(0 = MAF, 257 = Intake Boost pressure, 1 = Intake Atm Pressure)";
                            mapmafsh.CodeBlock = DetermineCodeBlockByByAddress(mapmafsh.Flash_start_address, newCodeBlocks);
                            newSymbols.Add(mapmafsh);
                            //Console.WriteLine("Found MAP MAF switch @ " + MAPMAFSwitch.ToString("X8"));
                            offset2 = MAPMAFSwitch + 1;
                        }
                    }


                    offset = SVBLAddress + 1;
                }

                else found = false;
            }
            return SVBLFound;
        }

        private bool FindVCDSIQDiag1(byte[] allBytes, string filename, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            bool found = true;
            bool DiagFound = false;
            int offset = 0;
            int offset2 = 0;
            while (found)
            {
                int DiagAddress = Tools.Instance.findSequence(allBytes, offset, new byte[10] { 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x69, 0x10, 0x0F, 0x05, 0x09 }, new byte[10] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });

                if (DiagAddress > 0)
                {

                    DiagFound = true;
                    SymbolHelper shvcdsdiagiq1 = new SymbolHelper();
                    shvcdsdiagiq1.Category = "Detected maps";
                    shvcdsdiagiq1.Subcategory = "Misc";
                    shvcdsdiagiq1.Flash_start_address = DiagAddress + 12;
                    shvcdsdiagiq1.Varname = "VCDS Diagnostic IQ Limit 1 [" + DetermineNumberByFlashBank(shvcdsdiagiq1.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagiq1.Length = 2;
                    shvcdsdiagiq1.X_axis_length = 1;
                    shvcdsdiagiq1.Y_axis_length = 1;
                    shvcdsdiagiq1.Offset = -0.15234375;
                    shvcdsdiagiq1.Correction = 0.00390625;
                    shvcdsdiagiq1.X_axis_descr = "IQ Limit in mg/st (whole numbers only!)";
                    shvcdsdiagiq1.XaxisUnits = "mg";
                    shvcdsdiagiq1.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagiq1.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagiq2 = new SymbolHelper();
                    shvcdsdiagiq2.Category = "Detected maps";
                    shvcdsdiagiq2.Subcategory = "Misc";
                    shvcdsdiagiq2.Flash_start_address = DiagAddress + 56;
                    shvcdsdiagiq2.Varname = "VCDS Diagnostic IQ Limit 2 [" + DetermineNumberByFlashBank(shvcdsdiagiq2.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagiq2.Length = 2;
                    shvcdsdiagiq2.X_axis_length = 1;
                    shvcdsdiagiq2.Y_axis_length = 1;
                    shvcdsdiagiq2.Offset = -0.15234375;
                    shvcdsdiagiq2.Correction = 0.00390625;
                    shvcdsdiagiq2.X_axis_descr = "IQ Limit in mg/st (whole numbers only!)";
                    shvcdsdiagiq2.XaxisUnits = "mg";
                    shvcdsdiagiq2.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagiq2.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagiq3 = new SymbolHelper();
                    shvcdsdiagiq3.Category = "Detected maps";
                    shvcdsdiagiq3.Subcategory = "Misc";
                    shvcdsdiagiq3.Flash_start_address = DiagAddress + 64;
                    shvcdsdiagiq3.Varname = "VCDS Diagnostic IQ Limit 3 [" + DetermineNumberByFlashBank(shvcdsdiagiq3.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagiq3.Length = 2;
                    shvcdsdiagiq3.X_axis_length = 1;
                    shvcdsdiagiq3.Y_axis_length = 1;
                    shvcdsdiagiq3.Offset = -0.15234375;
                    shvcdsdiagiq3.Correction = 0.00390625;
                    shvcdsdiagiq3.X_axis_descr = "IQ Limit in mg/st (whole numbers only!)";
                    shvcdsdiagiq3.XaxisUnits = "mg";
                    shvcdsdiagiq3.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagiq3.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagiq4 = new SymbolHelper();
                    shvcdsdiagiq4.Category = "Detected maps";
                    shvcdsdiagiq4.Subcategory = "Misc";
                    shvcdsdiagiq4.Flash_start_address = DiagAddress + 108;
                    shvcdsdiagiq4.Varname = "VCDS Diagnostic IQ Limit 4 [" + DetermineNumberByFlashBank(shvcdsdiagiq4.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagiq4.Length = 2;
                    shvcdsdiagiq4.X_axis_length = 1;
                    shvcdsdiagiq4.Y_axis_length = 1;
                    shvcdsdiagiq4.Offset = -0.15234375;
                    shvcdsdiagiq4.Correction = 0.00390625;
                    shvcdsdiagiq4.X_axis_descr = "IQ Limit in mg/st (whole numbers only!)";
                    shvcdsdiagiq4.XaxisUnits = "mg";
                    shvcdsdiagiq4.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagiq4.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagiq5 = new SymbolHelper();
                    shvcdsdiagiq5.Category = "Detected maps";
                    shvcdsdiagiq5.Subcategory = "Misc";
                    shvcdsdiagiq5.Flash_start_address = DiagAddress + 120;
                    shvcdsdiagiq5.Varname = "VCDS Diagnostic IQ Limit 5 [" + DetermineNumberByFlashBank(shvcdsdiagiq5.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagiq5.Length = 2;
                    shvcdsdiagiq5.X_axis_length = 1;
                    shvcdsdiagiq5.Y_axis_length = 1;
                    shvcdsdiagiq5.Offset = -0.15234375;
                    shvcdsdiagiq5.Correction = 0.00390625;
                    shvcdsdiagiq5.X_axis_descr = "IQ Limit in mg/st (whole numbers only!)";
                    shvcdsdiagiq5.XaxisUnits = "mg";
                    shvcdsdiagiq5.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagiq5.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagiq6 = new SymbolHelper();
                    shvcdsdiagiq6.Category = "Detected maps";
                    shvcdsdiagiq6.Subcategory = "Misc";
                    shvcdsdiagiq6.Flash_start_address = DiagAddress + 124;
                    shvcdsdiagiq6.Varname = "VCDS Diagnostic IQ Limit 6 [" + DetermineNumberByFlashBank(shvcdsdiagiq6.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagiq6.Length = 2;
                    shvcdsdiagiq6.X_axis_length = 1;
                    shvcdsdiagiq6.Y_axis_length = 1;
                    shvcdsdiagiq6.Offset = -0.15234375;
                    shvcdsdiagiq6.Correction = 0.00390625;
                    shvcdsdiagiq6.X_axis_descr = "IQ Limit in mg/st (whole numbers only!)";
                    shvcdsdiagiq6.XaxisUnits = "mg";
                    shvcdsdiagiq6.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagiq6.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagiq7 = new SymbolHelper();
                    shvcdsdiagiq7.Category = "Detected maps";
                    shvcdsdiagiq7.Subcategory = "Misc";
                    shvcdsdiagiq7.Flash_start_address = DiagAddress + 144;
                    shvcdsdiagiq7.Varname = "VCDS Diagnostic IQ Limit 7 [" + DetermineNumberByFlashBank(shvcdsdiagiq7.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagiq7.Length = 2;
                    shvcdsdiagiq7.X_axis_length = 1;
                    shvcdsdiagiq7.Y_axis_length = 1;
                    shvcdsdiagiq7.Offset = -0.15234375;
                    shvcdsdiagiq7.Correction = 0.00390625;
                    shvcdsdiagiq7.X_axis_descr = "IQ Limit in mg/st (whole numbers only!)";
                    shvcdsdiagiq7.XaxisUnits = "mg";
                    shvcdsdiagiq7.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagiq7.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagiq8 = new SymbolHelper();
                    shvcdsdiagiq8.Category = "Detected maps";
                    shvcdsdiagiq8.Subcategory = "Misc";
                    shvcdsdiagiq8.Flash_start_address = DiagAddress + 196;
                    shvcdsdiagiq8.Varname = "VCDS Diagnostic IQ Limit 8 [" + DetermineNumberByFlashBank(shvcdsdiagiq8.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagiq8.Length = 2;
                    shvcdsdiagiq8.X_axis_length = 1;
                    shvcdsdiagiq8.Y_axis_length = 1;
                    shvcdsdiagiq8.Offset = -0.15234375;
                    shvcdsdiagiq8.Correction = 0.00390625;
                    shvcdsdiagiq8.X_axis_descr = "IQ Limit in mg/st (whole numbers only!)";
                    shvcdsdiagiq8.XaxisUnits = "mg";
                    shvcdsdiagiq8.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagiq8.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagiq9 = new SymbolHelper();
                    shvcdsdiagiq9.Category = "Detected maps";
                    shvcdsdiagiq9.Subcategory = "Misc";
                    shvcdsdiagiq9.Flash_start_address = DiagAddress + 220;
                    shvcdsdiagiq9.Varname = "VCDS Diagnostic IQ Limit 9 [" + DetermineNumberByFlashBank(shvcdsdiagiq9.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagiq9.Length = 2;
                    shvcdsdiagiq9.X_axis_length = 1;
                    shvcdsdiagiq9.Y_axis_length = 1;
                    shvcdsdiagiq9.Offset = -0.15234375;
                    shvcdsdiagiq9.Correction = 0.00390625;
                    shvcdsdiagiq9.X_axis_descr = "IQ Limit in mg/st (whole numbers only!)";
                    shvcdsdiagiq9.XaxisUnits = "mg";
                    shvcdsdiagiq9.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagiq9.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagiq10 = new SymbolHelper();
                    shvcdsdiagiq10.Category = "Detected maps";
                    shvcdsdiagiq10.Subcategory = "Misc";
                    shvcdsdiagiq10.Flash_start_address = DiagAddress + 228;
                    shvcdsdiagiq10.Varname = "VCDS Diagnostic IQ Limit 10 [" + DetermineNumberByFlashBank(shvcdsdiagiq10.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagiq10.Length = 2;
                    shvcdsdiagiq10.X_axis_length = 1;
                    shvcdsdiagiq10.Y_axis_length = 1;
                    shvcdsdiagiq10.Offset = -0.15234375;
                    shvcdsdiagiq10.Correction = 0.00390625;
                    shvcdsdiagiq10.X_axis_descr = "IQ Limit in mg/st (whole numbers only!)";
                    shvcdsdiagiq10.XaxisUnits = "mg";
                    shvcdsdiagiq10.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagiq10.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagmaf1 = new SymbolHelper();
                    shvcdsdiagmaf1.Category = "Detected maps";
                    shvcdsdiagmaf1.Subcategory = "Misc";
                    shvcdsdiagmaf1.Flash_start_address = DiagAddress + 24;
                    shvcdsdiagmaf1.Varname = "VCDS Diagnostic MAF Limit 1 [" + DetermineNumberByFlashBank(shvcdsdiagmaf1.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagmaf1.Length = 2;
                    shvcdsdiagmaf1.X_axis_length = 1;
                    shvcdsdiagmaf1.Y_axis_length = 1;
                    shvcdsdiagmaf1.Offset = -1.225;
                    shvcdsdiagmaf1.Correction = 0.025;
                    shvcdsdiagmaf1.X_axis_descr = "MAF Limit in mg/h (only multiplies of 32 works, 800 MIN and 1504 MAX!)";
                    shvcdsdiagmaf1.XaxisUnits = "mg/h";
                    shvcdsdiagmaf1.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagmaf1.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagmaf2 = new SymbolHelper();
                    shvcdsdiagmaf2.Category = "Detected maps";
                    shvcdsdiagmaf2.Subcategory = "Misc";
                    shvcdsdiagmaf2.Flash_start_address = DiagAddress + 76;
                    shvcdsdiagmaf2.Varname = "VCDS Diagnostic MAF Limit 2 [" + DetermineNumberByFlashBank(shvcdsdiagmaf2.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagmaf2.Length = 2;
                    shvcdsdiagmaf2.X_axis_length = 1;
                    shvcdsdiagmaf2.Y_axis_length = 1;
                    shvcdsdiagmaf2.Offset = -1.225;
                    shvcdsdiagmaf2.Correction = 0.025;
                    shvcdsdiagmaf2.X_axis_descr = "MAF Limit in mg/h (only multiplies of 32 works, 800 MIN and 1504 MAX!)";
                    shvcdsdiagmaf2.XaxisUnits = "mg/h";
                    shvcdsdiagmaf2.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagmaf2.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagpre1 = new SymbolHelper();
                    shvcdsdiagpre1.Category = "Detected maps";
                    shvcdsdiagpre1.Subcategory = "Misc";
                    shvcdsdiagpre1.Flash_start_address = DiagAddress + 28;
                    shvcdsdiagpre1.Varname = "VCDS Diagnostic MAP Limit 1 [" + DetermineNumberByFlashBank(shvcdsdiagpre1.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagpre1.Length = 2;
                    shvcdsdiagpre1.X_axis_length = 1;
                    shvcdsdiagpre1.Y_axis_length = 1;
                    //shvcdsdiagpre1.Offset = -1.225;
                    //shvcdsdiagpre1.Correction = 0.025;
                    shvcdsdiagpre1.X_axis_descr = "MAP Limit in mbar (enter -238 (default), 29960 (3bar MAP sensor), 40200 (4bar MAP sensor))";
                    //shvcdsdiagpre1.XaxisUnits = "mbar";
                    shvcdsdiagpre1.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagpre1.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagpre2 = new SymbolHelper();
                    shvcdsdiagpre2.Category = "Detected maps";
                    shvcdsdiagpre2.Subcategory = "Misc";
                    shvcdsdiagpre2.Flash_start_address = DiagAddress + 32;
                    shvcdsdiagpre2.Varname = "VCDS Diagnostic MAP Limit 2 [" + DetermineNumberByFlashBank(shvcdsdiagpre2.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagpre2.Length = 2;
                    shvcdsdiagpre2.X_axis_length = 1;
                    shvcdsdiagpre2.Y_axis_length = 1;
                    //shvcdsdiagpre1.Offset = -1.225;
                    //shvcdsdiagpre1.Correction = 0.025;
                    shvcdsdiagpre2.X_axis_descr = "MAP Limit in mbar (enter -238 (default), 29960 (3bar MAP sensor), 40200 (4bar MAP sensor))";
                    //shvcdsdiagpre1.XaxisUnits = "mbar";
                    shvcdsdiagpre2.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagpre2.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagpre3 = new SymbolHelper();
                    shvcdsdiagpre3.Category = "Detected maps";
                    shvcdsdiagpre3.Subcategory = "Misc";
                    shvcdsdiagpre3.Flash_start_address = DiagAddress + 72;
                    shvcdsdiagpre3.Varname = "VCDS Diagnostic MAP Limit 3 [" + DetermineNumberByFlashBank(shvcdsdiagpre3.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagpre3.Length = 2;
                    shvcdsdiagpre3.X_axis_length = 1;
                    shvcdsdiagpre3.Y_axis_length = 1;
                    //shvcdsdiagpre1.Offset = -1.225;
                    //shvcdsdiagpre1.Correction = 0.025;
                    shvcdsdiagpre3.X_axis_descr = "MAP Limit in mbar (enter -238 (default), 29960 (3bar MAP sensor), 40200 (4bar MAP sensor))";
                    //shvcdsdiagpre1.XaxisUnits = "mbar";
                    shvcdsdiagpre3.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagpre3.Flash_start_address, newCodeBlocks);

                    SymbolHelper shvcdsdiagtor1 = new SymbolHelper();
                    shvcdsdiagtor1.Category = "Detected maps";
                    shvcdsdiagtor1.Subcategory = "Misc";
                    shvcdsdiagtor1.Flash_start_address = DiagAddress + 188;
                    shvcdsdiagtor1.Varname = "VCDS Diagnostic Torque Limit [" + DetermineNumberByFlashBank(shvcdsdiagtor1.Flash_start_address, newCodeBlocks) + "]";
                    shvcdsdiagtor1.Length = 2;
                    shvcdsdiagtor1.X_axis_length = 1;
                    shvcdsdiagtor1.Y_axis_length = 1;
                    shvcdsdiagtor1.Offset = -0.203125;
                    shvcdsdiagtor1.Correction = 0.00390625;
                    shvcdsdiagtor1.X_axis_descr = "Torque Limit in NM (real torque is 4,12x the value so 412NM for 100, 146 for 601,5nm, 195 for 803,4nm etc.)";
                    shvcdsdiagtor1.XaxisUnits = "NM";
                    shvcdsdiagtor1.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagtor1.Flash_start_address, newCodeBlocks);


                    newSymbols.Add(shvcdsdiagiq1);
                    newSymbols.Add(shvcdsdiagiq2);
                    newSymbols.Add(shvcdsdiagiq3);
                    newSymbols.Add(shvcdsdiagiq4);
                    newSymbols.Add(shvcdsdiagiq5);
                    newSymbols.Add(shvcdsdiagiq6);
                    newSymbols.Add(shvcdsdiagiq7);
                    newSymbols.Add(shvcdsdiagiq8);
                    newSymbols.Add(shvcdsdiagiq9);
                    newSymbols.Add(shvcdsdiagiq10);
                    newSymbols.Add(shvcdsdiagmaf1);
                    newSymbols.Add(shvcdsdiagmaf2);
                    newSymbols.Add(shvcdsdiagpre1);
                    newSymbols.Add(shvcdsdiagpre2);
                    newSymbols.Add(shvcdsdiagpre3);
                    newSymbols.Add(shvcdsdiagtor1);

                    
                    int VCDS16BitLimits = Tools.Instance.findSequence(allBytes, offset2, new byte[10] { 0xD4, 0xFE, 0x91, 0x00, 0xB6, 0xFE, 0x8C, 0x05, 0xB6, 0xFE }, new byte[10] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
                    if (VCDS16BitLimits > 0)
                    {
                        SymbolHelper shvcdsdiagtorlim = new SymbolHelper();
                        shvcdsdiagtorlim.Category = "Detected maps";
                        shvcdsdiagtorlim.Subcategory = "Misc";
                        shvcdsdiagtorlim.Flash_start_address = VCDS16BitLimits + 42;
                        shvcdsdiagtorlim.Varname = "VCDS Diagnostic Torque Display offset [" + DetermineNumberByFlashBank(shvcdsdiagtorlim.Flash_start_address, newCodeBlocks) + "]";
                        shvcdsdiagtorlim.Length = 2;
                        shvcdsdiagtorlim.X_axis_length = 1;
                        shvcdsdiagtorlim.Y_axis_length = 1;
                        //shvcdsdiagtorlim.Offset = -0.203125;
                        //shvcdsdiagtorlim.Correction = 0.00390625;
                        shvcdsdiagtorlim.X_axis_descr = "Enter X where 255/Maximum Torque Value * 1000 = X (ie. 255/412NM*1000=619)";
                        //shvcdsdiagtorlim.XaxisUnits = "NM";
                        shvcdsdiagtorlim.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagtorlim.Flash_start_address, newCodeBlocks);
                        //Console.WriteLine("Found torque offset " + VCDS16BitLimits.ToString("X8") + " " + VCDS16BitLimits + 168.ToString("X8"));

                        SymbolHelper shvcdsdiagmaflim = new SymbolHelper();
                        shvcdsdiagmaflim.Category = "Detected maps";
                        shvcdsdiagmaflim.Subcategory = "Misc";
                        shvcdsdiagmaflim.Flash_start_address = VCDS16BitLimits + 54;
                        shvcdsdiagmaflim.Varname = "VCDS Diagnostic MAF Display offset [" + DetermineNumberByFlashBank(shvcdsdiagmaflim.Flash_start_address, newCodeBlocks) + "]";
                        shvcdsdiagmaflim.Length = 2;
                        shvcdsdiagmaflim.X_axis_length = 1;
                        shvcdsdiagmaflim.Y_axis_length = 1;
                        //shvcdsdiagtorlim.Offset = -0.203125;
                        //shvcdsdiagtorlim.Correction = 0.00390625;
                        shvcdsdiagmaflim.X_axis_descr = "Enter X where 255/Maximum MAF Value * 1000 = X (ie. 255/1250mg*1000=204)";
                        //shvcdsdiagtorlim.XaxisUnits = "NM";
                        shvcdsdiagmaflim.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagmaflim.Flash_start_address, newCodeBlocks);

                        SymbolHelper shvcdsdiagprelim = new SymbolHelper();
                        shvcdsdiagprelim.Category = "Detected maps";
                        shvcdsdiagprelim.Subcategory = "Misc";
                        shvcdsdiagprelim.Flash_start_address = VCDS16BitLimits + 66;
                        shvcdsdiagprelim.Varname = "VCDS Diagnostic MAP Display offset [" + DetermineNumberByFlashBank(shvcdsdiagprelim.Flash_start_address, newCodeBlocks) + "]";
                        shvcdsdiagprelim.Length = 2;
                        shvcdsdiagprelim.X_axis_length = 1;
                        shvcdsdiagprelim.Y_axis_length = 1;
                        //shvcdsdiagtorlim.Offset = -0.203125;
                        //shvcdsdiagtorlim.Correction = 0.00390625;
                        shvcdsdiagprelim.X_axis_descr = "Enter X where 255/Maximum MAP Value * 10000 = X (ie. 255/4000Mbar*10000=638)";
                        //shvcdsdiagtorlim.XaxisUnits = "NM";
                        shvcdsdiagprelim.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagprelim.Flash_start_address, newCodeBlocks);

                        SymbolHelper shvcdsdiagiqlim = new SymbolHelper();
                        shvcdsdiagiqlim.Category = "Detected maps";
                        shvcdsdiagiqlim.Subcategory = "Misc";
                        shvcdsdiagiqlim.Flash_start_address = VCDS16BitLimits + 138;
                        shvcdsdiagiqlim.Varname = "VCDS Diagnostic IQ Display offset [" + DetermineNumberByFlashBank(shvcdsdiagiqlim.Flash_start_address, newCodeBlocks) + "]";
                        shvcdsdiagiqlim.Length = 2;
                        shvcdsdiagiqlim.X_axis_length = 1;
                        shvcdsdiagiqlim.Y_axis_length = 1;
                        //shvcdsdiagtorlim.Offset = -0.203125;
                        //shvcdsdiagtorlim.Correction = 0.00390625;
                        shvcdsdiagiqlim.X_axis_descr = "Enter X where 255/Maximum IQ Value * 100 = X (ie. 255/100mg*100=255)";
                        //shvcdsdiagtorlim.XaxisUnits = "NM";
                        shvcdsdiagiqlim.CodeBlock = DetermineCodeBlockByByAddress(shvcdsdiagiqlim.Flash_start_address, newCodeBlocks);

                        newSymbols.Add(shvcdsdiagtorlim);
                        newSymbols.Add(shvcdsdiagmaflim);
                        newSymbols.Add(shvcdsdiagprelim);
                        newSymbols.Add(shvcdsdiagiqlim);
                        offset2 = VCDS16BitLimits + 1;
                    }
                    

                    offset = DiagAddress + 1;
                }

                else found = false;
            }
            return DiagFound;
        }

        private bool FindSVRLSequence(byte[] allBytes, string filename, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            bool found = true;
            bool SVRLFound = false;
            int offset = 0;
            int offset2 = 0;
            while (found)
            {
                int SVRLAddress = Tools.Instance.findSequence(allBytes, offset, new byte[8] { 0x00, 0x0E, 0x24, 0x13, 0x32, 0x00, 0x70, 0x17 }, new byte[8] { 1, 1, 0, 0, 1, 1, 1, 1 });

                if (SVRLAddress > 0)
                {
                    //Console.WriteLine("Alternative SVBL " + SVBLAddress.ToString("X8"));
                    SVRLFound = true;
                    SymbolHelper shsvrl = new SymbolHelper();
                    shsvrl.Category = "Detected maps";
                    shsvrl.Subcategory = "Limiters";
                    shsvrl.Flash_start_address = SVRLAddress + 2;

                    shsvrl.Varname = "SVRL (Single Value Rev Limiter ) [" + DetermineNumberByFlashBank(shsvrl.Flash_start_address, newCodeBlocks) + "]";
                    shsvrl.Length = 2;
                    shsvrl.CodeBlock = DetermineCodeBlockByByAddress(shsvrl.Flash_start_address, newCodeBlocks);
                    newSymbols.Add(shsvrl);

                    
                    int LeftFootBrakeSwitch = Tools.Instance.findSequence(allBytes, offset2, new byte[6] { 0x27, 0x00, 0x00, 0x64, 0x00, 0x01 }, new byte[6] { 1, 1, 1, 1, 1, 0 });
                    if (LeftFootBrakeSwitch > 0)
                    {
                        SymbolHelper lfbsh = new SymbolHelper();
                        lfbsh.Category = "Detected maps";
                        lfbsh.Subcategory = "Switches";
                        lfbsh.Flash_start_address = LeftFootBrakeSwitch + 5;
                        lfbsh.Varname = "Left Foot Brake Switch (1 = ON (default), O = OFF)" + DetermineNumberByFlashBank(lfbsh.Flash_start_address, newCodeBlocks);
                        lfbsh.Length = 2;
                        lfbsh.CodeBlock = DetermineCodeBlockByByAddress(lfbsh.Flash_start_address, newCodeBlocks);
                        newSymbols.Add(lfbsh);
                        offset2 = LeftFootBrakeSwitch + 1;
                    }
                    

                    offset = SVRLAddress + 1;
                }

                else found = false;
            }
            return SVRLFound;
        }

        private bool FindPIDmaps(byte[] allBytes, string filename, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks) //Deprecated as Now I can find them automatically
        {
            bool found = true;
            bool PIDFound = false;
            int offset = 0;
            int pidnr = 0;
            while (found)
            {
                int PIDAddress = Tools.Instance.findSequence(allBytes, offset, new byte[8] { 0x00, 0x00, 0x85, 0x00, 0x0A, 0x01, 0x8F, 0x01 }, new byte[8] { 1, 1, 1, 1, 1, 1, 1, 1 });

                if (PIDAddress > 0)
                {
                    //Console.WriteLine("Alternative SVBL " + SVBLAddress.ToString("X8"));
                    PIDFound = true;

                    if (pidnr % 4 == 0)
                    {

                        SymbolHelper shpid = new SymbolHelper();
                        shpid.Category = "Detected maps";
                        shpid.Subcategory = "Turbo";
                        shpid.Flash_start_address = PIDAddress + 32;

                        shpid.Varname = "PID map [" + DetermineNumberByFlashBank(shpid.Flash_start_address, newCodeBlocks) + "]";
                        shpid.Length = 32;
                        shpid.X_axis_length = 16;
                        shpid.Y_axis_length = 1;
                        shpid.X_axis_address = PIDAddress;
                        shpid.CodeBlock = DetermineCodeBlockByByAddress(shpid.Flash_start_address, newCodeBlocks);
                        newSymbols.Add(shpid);
                        pidnr++;
                    }
                    else if (pidnr % 4 == 1)
                    {

                        SymbolHelper shpid = new SymbolHelper();
                        shpid.Category = "Detected maps";
                        shpid.Subcategory = "Turbo";
                        shpid.Flash_start_address = PIDAddress + 32;

                        shpid.Varname = "PID map I [" + DetermineNumberByFlashBank(shpid.Flash_start_address, newCodeBlocks) + "]";
                        shpid.Length = 32;
                        shpid.X_axis_length = 16;
                        shpid.Y_axis_length = 1;
                        shpid.X_axis_address = PIDAddress;
                        shpid.CodeBlock = DetermineCodeBlockByByAddress(shpid.Flash_start_address, newCodeBlocks);
                        newSymbols.Add(shpid);
                        pidnr++;
                    }
                    else if (pidnr % 4 == 2)
                    {

                        SymbolHelper shpid = new SymbolHelper();
                        shpid.Category = "Detected maps";
                        shpid.Subcategory = "Turbo";
                        shpid.Flash_start_address = PIDAddress + 32;

                        shpid.Varname = "PID map D [" + DetermineNumberByFlashBank(shpid.Flash_start_address, newCodeBlocks) + "]";
                        shpid.Length = 32;
                        shpid.X_axis_length = 16;
                        shpid.Y_axis_length = 1;
                        shpid.X_axis_address = PIDAddress;
                        shpid.CodeBlock = DetermineCodeBlockByByAddress(shpid.Flash_start_address, newCodeBlocks);
                        newSymbols.Add(shpid);
                        pidnr++;
                    }
                    else if (pidnr % 4 == 3)
                    {

                        SymbolHelper shpid = new SymbolHelper();
                        shpid.Category = "Detected maps";
                        shpid.Subcategory = "Turbo";
                        shpid.Flash_start_address = PIDAddress + 32;

                        shpid.Varname = "PID map P [" + DetermineNumberByFlashBank(shpid.Flash_start_address, newCodeBlocks) + "]";
                        shpid.Length = 32;
                        shpid.X_axis_length = 16;
                        shpid.Y_axis_length = 1;
                        shpid.X_axis_address = PIDAddress;
                        shpid.CodeBlock = DetermineCodeBlockByByAddress(shpid.Flash_start_address, newCodeBlocks);
                        newSymbols.Add(shpid);
                        pidnr++;
                    }

                    offset = PIDAddress + 1;
                }

                else found = false;
            }
            return PIDFound;
        }

        private bool FindBIPline(byte[] allBytes, string filename, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            bool found = true;
            bool BIPLineFound = false;
            int offset = 0;
            int offset2 = 0;
            while (found)
            {
                int BIPLineAddress = Tools.Instance.findSequence(allBytes, offset, new byte[8] { 0x0A, 0x00, 0x58, 0x01, 0x89, 0x01, 0xBA, 0x01 }, new byte[8] { 1, 1, 1, 1, 1, 1, 1, 1 });

                if (BIPLineAddress > 0)
                {
                   
                    BIPLineFound = true;
                    SymbolHelper shbipl = new SymbolHelper();
                    shbipl.Category = "Detected maps";
                    shbipl.Subcategory = "Fuel";
                    shbipl.Flash_start_address = BIPLineAddress + 22;

                    shbipl.Varname = "BIP Basic characteristic line [" + DetermineNumberByFlashBank(shbipl.Flash_start_address, newCodeBlocks) + "]";
                    shbipl.Length = 20;
                    shbipl.X_axis_length = 10;
                    shbipl.Y_axis_length = 1;
                    shbipl.X_axis_address = BIPLineAddress + 2;
                    shbipl.YaxisUnits = "V";
                    shbipl.Z_axis_descr = "Time in microseconds";
                    shbipl.Y_axis_descr = "Battery voltage in V";
                    shbipl.Y_axis_correction = 0.0203147;
                    shbipl.CodeBlock = DetermineCodeBlockByByAddress(shbipl.Flash_start_address, newCodeBlocks);

                    newSymbols.Add(shbipl);

                    int BIPTLineAddress = Tools.Instance.findSequence(allBytes, offset2, new byte[8] { 0x0A, 0x00, 0x4D, 0x09, 0xE3, 0x09, 0x47, 0x0A }, new byte[8] { 1, 1, 1, 1, 1, 1, 1, 1 });
                    if (BIPTLineAddress > 0)
                    {
                        SymbolHelper shbiptl = new SymbolHelper();
                        shbiptl.Category = "Detected maps";
                        shbiptl.Subcategory = "Fuel";
                        shbiptl.Flash_start_address = BIPTLineAddress + 22;

                        shbiptl.Varname = "BIP temperature correction [" + DetermineNumberByFlashBank(shbiptl.Flash_start_address, newCodeBlocks) + "]";
                        shbiptl.Length = 20;
                        shbiptl.X_axis_length = 10;
                        shbiptl.Y_axis_length = 1;
                        shbiptl.X_axis_address = BIPTLineAddress + 2;
                        shbiptl.YaxisUnits = "°C";
                        shbiptl.Z_axis_descr = "Crankshaft °";
                        shbiptl.Y_axis_descr = "Temperature in °C";
                        shbiptl.Y_axis_correction = 0.1;
                        shbiptl.Y_axis_offset = -273.1;
                        shbiptl.Correction = 0.000244;
                        shbiptl.CodeBlock = DetermineCodeBlockByByAddress(shbiptl.Flash_start_address, newCodeBlocks);

                        newSymbols.Add(shbiptl);

                        offset2 = BIPTLineAddress + 1;
                    }

                  

                    offset = BIPLineAddress + 1;
                }

                else found = false;
            }
            return BIPLineFound;
        }

        //Deprecated
        private bool Findmrwmaps1(byte[] allBytes, string filename, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            bool found = true;
            bool mrwKTBKFfound = false;
            //bool mrwMERKFfound = false;
            int offset = 0;
            int offset2 = 0;
            while (found)
            {
                int mrwKTBKFAddress = Tools.Instance.findSequence(allBytes, offset, new byte[10] { 0xA6, 0x0B, 0x96, 0x0F, 0x88, 0x13, 0x44, 0xF9, 0x0A, 0x00 }, new byte[10] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });

                if (mrwKTBKFAddress > 0)
                {

                    mrwKTBKFfound = true;
                    SymbolHelper shmrwKTBKF = new SymbolHelper();
                    shmrwKTBKF.Category = "Detected maps";
                    shmrwKTBKF.Subcategory = "Misc";
                    shmrwKTBKF.Flash_start_address = mrwKTBKFAddress + 30;

                    shmrwKTBKF.Varname = "Injected Fuel Volume Correction at 100°C (mrwKTB_KF) [" + DetermineNumberByFlashBank(shmrwKTBKF.Flash_start_address, newCodeBlocks) + "]";
                    shmrwKTBKF.Length = 200;
                    shmrwKTBKF.X_axis_length = 10;
                    shmrwKTBKF.Y_axis_length = 10;
                    shmrwKTBKF.X_axis_address = mrwKTBKFAddress + 10;
                    shmrwKTBKF.Y_axis_address = mrwKTBKFAddress - 14;
                    shmrwKTBKF.XaxisUnits = "RPM";
                    shmrwKTBKF.YaxisUnits = "mg/st";
                    shmrwKTBKF.Z_axis_descr = "IQ correction (mg/st)";
                    shmrwKTBKF.X_axis_descr = "Engine speed (RPM)";
                    shmrwKTBKF.Y_axis_descr = "Injected Quantity (mg/stroke)";
                    shmrwKTBKF.Y_axis_correction = 0.01;
                    shmrwKTBKF.Correction = 0.01;
                    shmrwKTBKF.CodeBlock = DetermineCodeBlockByByAddress(shmrwKTBKF.Flash_start_address, newCodeBlocks);

                    newSymbols.Add(shmrwKTBKF);

                    offset = mrwKTBKFAddress + 1;
                }
                /* Deprecated
                int mrwMERKFAddress = Tools.Instance.findSequence(allBytes, offset2, new byte[6] { 0x2C, 0x01, 0x2C, 0x01, 0x2E, 0xEC }, new byte[6] { 1, 1, 1, 1, 1, 1 });
                
                if (mrwMERKFAddress > 0)
                {

                    mrwMERKFfound = true;
                    SymbolHelper shmrwMERKF = new SymbolHelper();
                    shmrwMERKF.Category = "Detected maps";
                    shmrwMERKF.Subcategory = "Misc";
                    shmrwMERKF.Flash_start_address = mrwMERKFAddress + 52;

                    shmrwMERKF.Varname = "AG4 Transmission downshift map (mrwM_ER_KF) [" + DetermineNumberByFlashBank(shmrwMERKF.Flash_start_address, newCodeBlocks) + "]";
                    shmrwMERKF.Length = 200;
                    shmrwMERKF.X_axis_length = 10;
                    shmrwMERKF.Y_axis_length = 10;
                    shmrwMERKF.X_axis_address = mrwMERKFAddress + 32;
                    shmrwMERKF.Y_axis_address = mrwMERKFAddress + 8;
                    shmrwMERKF.XaxisUnits = "RPM";
                    shmrwMERKF.YaxisUnits = "mg/st";
                    shmrwMERKF.Z_axis_descr = "IQ (mg/stroke)";
                    shmrwMERKF.X_axis_descr = "Engine speed (RPM)";
                    shmrwMERKF.Y_axis_descr = "Injected Quantity (mg/stroke)";
                    shmrwMERKF.Y_axis_correction = 0.01;
                    shmrwMERKF.Correction = 0.01;
                    shmrwMERKF.CodeBlock = DetermineCodeBlockByByAddress(shmrwMERKF.Flash_start_address, newCodeBlocks);

                    newSymbols.Add(shmrwMERKF);

                    SymbolHelper shmrwMEHKF = new SymbolHelper();
                    shmrwMEHKF.Category = "Detected maps";
                    shmrwMEHKF.Subcategory = "Misc";
                    shmrwMEHKF.Flash_start_address = mrwMERKFAddress + 300;

                    shmrwMEHKF.Varname = "AG4 Transmission upshift map (mrwM_EH_KF) [" + DetermineNumberByFlashBank(shmrwMEHKF.Flash_start_address, newCodeBlocks) + "]";
                    shmrwMEHKF.Length = 200;
                    shmrwMEHKF.X_axis_length = 10;
                    shmrwMEHKF.Y_axis_length = 10;
                    shmrwMEHKF.X_axis_address = mrwMERKFAddress + 280;
                    shmrwMEHKF.Y_axis_address = mrwMERKFAddress + 256;
                    shmrwMEHKF.XaxisUnits = "RPM";
                    shmrwMEHKF.YaxisUnits = "mg/st";
                    shmrwMEHKF.Z_axis_descr = "IQ (mg/stroke)";
                    shmrwMEHKF.X_axis_descr = "Engine speed (RPM)";
                    shmrwMEHKF.Y_axis_descr = "Injected Quantity (mg/stroke)";
                    shmrwMEHKF.Y_axis_correction = 0.01;
                    shmrwMEHKF.Correction = 0.01;
                    shmrwMEHKF.CodeBlock = DetermineCodeBlockByByAddress(shmrwMEHKF.Flash_start_address, newCodeBlocks);

                    newSymbols.Add(shmrwMEHKF);

                    offset2 = mrwMERKFAddress + 1;
                }*/

                else found = false;
            }
            return mrwKTBKFfound;
        }

        private bool Findmrwmaps2(byte[] allBytes, string filename, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            bool found = true;
            bool fnwDZfound = false;
            bool mrwDZfound = false;
            bool mrwMERKFfound = false;
            int offset = 0;
            int offset2 = 0;
            int offset3 = 0;
            while (found)
            {
                int fnwDZAddress = Tools.Instance.findSequence(allBytes, offset, new byte[5] { 0x4A, 0xF9, 0x10, 0x00, 0x64 }, new byte[5] { 1, 1, 1, 1, 1 });
                
                if (fnwDZAddress > 0)
                {

                    fnwDZfound = true;
                    SymbolHelper shfnwDZ = new SymbolHelper();
                    shfnwDZ.Category = "Detected maps";
                    shfnwDZ.Subcategory = "Misc";
                    shfnwDZ.Flash_start_address = fnwDZAddress + 4;

                    shfnwDZ.Varname = "Grid table for speed input dzmNmit (fnwDZstzv) [" + DetermineNumberByFlashBank(shfnwDZ.Flash_start_address, newCodeBlocks) + "]";
                    shfnwDZ.Length = 32;
                    shfnwDZ.X_axis_length = 16;
                    shfnwDZ.Y_axis_length = 1;
                    shfnwDZ.Z_axis_descr = "Engine speed (rpm)";
                    shfnwDZ.X_axis_descr = "Points";
                    shfnwDZ.CodeBlock = DetermineCodeBlockByByAddress(shfnwDZ.Flash_start_address, newCodeBlocks);

                    SymbolHelper shfnwME = new SymbolHelper();
                    shfnwME.Category = "Detected maps";
                    shfnwME.Subcategory = "Misc";
                    shfnwME.Flash_start_address = fnwDZAddress + 40;

                    shfnwME.Varname = "Grid table for duration quantity fnoM_E (fnwMEstzv) [" + DetermineNumberByFlashBank(shfnwME.Flash_start_address, newCodeBlocks) + "]";
                    shfnwME.Length = 28;
                    shfnwME.X_axis_length = 14;
                    shfnwME.Y_axis_length = 1;
                    shfnwME.Z_axis_descr = "Injected Quantity (mg/stroke)";
                    shfnwME.X_axis_descr = "Points";
                    shfnwME.Correction = 0.01;
                    shfnwME.CodeBlock = DetermineCodeBlockByByAddress(shfnwME.Flash_start_address, newCodeBlocks);

                    SymbolHelper shfnwWT = new SymbolHelper();
                    shfnwWT.Category = "Detected maps";
                    shfnwWT.Subcategory = "Misc";
                    shfnwWT.Flash_start_address = fnwDZAddress + 72;

                    shfnwWT.Varname = "Grid table for water temperaure fnmWTF (fnwWTstzv) [" + DetermineNumberByFlashBank(shfnwWT.Flash_start_address, newCodeBlocks) + "]";
                    shfnwWT.Length = 20;
                    shfnwWT.X_axis_length = 10;
                    shfnwWT.Y_axis_length = 1;
                    shfnwWT.Z_axis_descr = "Injected Quantity (mg/stroke)";
                    shfnwWT.X_axis_descr = "Points";
                    shfnwWT.Correction = 0.1;
                    shfnwWT.Offset = -273.1;
                    shfnwWT.CodeBlock = DetermineCodeBlockByByAddress(shfnwWT.Flash_start_address, newCodeBlocks);

                    newSymbols.Add(shfnwDZ);
                    newSymbols.Add(shfnwME);
                    newSymbols.Add(shfnwWT);

                    offset = fnwDZAddress + 1;
                }

                int mrwDZAddress = Tools.Instance.findSequence(allBytes, offset3, new byte[5] { 0x4A, 0xF9, 0x10, 0x00, 0x5D }, new byte[5] { 1, 1, 1, 1, 1 });

                if (mrwDZAddress > 0)
                {

                    mrwDZfound = true;
                    SymbolHelper shmrwDZ = new SymbolHelper();
                    shmrwDZ.Category = "Detected maps";
                    shmrwDZ.Subcategory = "Misc";
                    shmrwDZ.Flash_start_address = mrwDZAddress + 4;

                    shmrwDZ.Varname = "Grid table for speed input dzmNmit (mrwDZstzv) [" + DetermineNumberByFlashBank(shmrwDZ.Flash_start_address, newCodeBlocks) + "]";
                    shmrwDZ.Length = 32;
                    shmrwDZ.X_axis_length = 16;
                    shmrwDZ.Y_axis_length = 1;
                    shmrwDZ.Z_axis_descr = "Engine speed (rpm)";
                    shmrwDZ.X_axis_descr = "Points";
                    shmrwDZ.CodeBlock = DetermineCodeBlockByByAddress(shmrwDZ.Flash_start_address, newCodeBlocks);

                    SymbolHelper shmrwLK = new SymbolHelper();
                    shmrwLK.Category = "Detected maps";
                    shmrwLK.Subcategory = "Misc";
                    shmrwLK.Flash_start_address = mrwDZAddress + 40;

                    shmrwLK.Varname = "Grid table for corrected airmass mroM_LK (mrwLKstzv) [" + DetermineNumberByFlashBank(shmrwLK.Flash_start_address, newCodeBlocks) + "]";
                    shmrwLK.Length = 26;
                    shmrwLK.X_axis_length = 13;
                    shmrwLK.Y_axis_length = 1;
                    shmrwLK.Z_axis_descr = "Airmass (mg/stroke)";
                    shmrwLK.X_axis_descr = "Points";
                    shmrwLK.Correction = 0.1;
                    shmrwLK.CodeBlock = DetermineCodeBlockByByAddress(shmrwLK.Flash_start_address, newCodeBlocks);

                    newSymbols.Add(shmrwDZ);
                    newSymbols.Add(shmrwLK);

                    offset3 = mrwDZAddress + 1;
                }
                /*
                int mrwMERKFAddress = Tools.Instance.findSequence(allBytes, offset2, new byte[8] { 0x10, 0x27, 0x10, 0x27, 0x2C, 0xEC, 0x0A, 0x00 }, new byte[8] { 1, 1, 1, 1, 1, 1, 1, 1 });

                if (mrwMERKFAddress > 0)
                {

                    mrwMERKFfound = true;
                    SymbolHelper shmrwMERKF = new SymbolHelper();
                    shmrwMERKF.Category = "Detected maps";
                    shmrwMERKF.Subcategory = "Misc";
                    shmrwMERKF.Flash_start_address = mrwMERKFAddress + 52;

                    shmrwMERKF.Varname = "AG4 Transmission downshift map (mrwM_ER_KF) [" + DetermineNumberByFlashBank(shmrwMERKF.Flash_start_address, newCodeBlocks) + "]";
                    shmrwMERKF.Length = 200;
                    shmrwMERKF.X_axis_length = 10;
                    shmrwMERKF.Y_axis_length = 10;
                    shmrwMERKF.X_axis_address = mrwMERKFAddress + 32;
                    shmrwMERKF.Y_axis_address = mrwMERKFAddress + 8;
                    shmrwMERKF.XaxisUnits = "RPM";
                    shmrwMERKF.YaxisUnits = "mg/st";
                    shmrwMERKF.Z_axis_descr = "IQ (mg/stroke)";
                    shmrwMERKF.X_axis_descr = "Engine speed (RPM)";
                    shmrwMERKF.Y_axis_descr = "Injected Quantity (mg/stroke)";
                    shmrwMERKF.Y_axis_correction = 0.01;
                    shmrwMERKF.Correction = 0.01;
                    shmrwMERKF.CodeBlock = DetermineCodeBlockByByAddress(shmrwMERKF.Flash_start_address, newCodeBlocks);

                    newSymbols.Add(shmrwMERKF);

                    SymbolHelper shmrwMEHKF = new SymbolHelper();
                    shmrwMEHKF.Category = "Detected maps";
                    shmrwMEHKF.Subcategory = "Misc";
                    shmrwMEHKF.Flash_start_address = mrwMERKFAddress + 300;

                    shmrwMEHKF.Varname = "AG4 Transmission upshift map (mrwM_EH_KF) [" + DetermineNumberByFlashBank(shmrwMEHKF.Flash_start_address, newCodeBlocks) + "]";
                    shmrwMEHKF.Length = 200;
                    shmrwMEHKF.X_axis_length = 10;
                    shmrwMEHKF.Y_axis_length = 10;
                    shmrwMEHKF.X_axis_address = mrwMERKFAddress + 280;
                    shmrwMEHKF.Y_axis_address = mrwMERKFAddress + 256;
                    shmrwMEHKF.XaxisUnits = "RPM";
                    shmrwMEHKF.YaxisUnits = "mg/st";
                    shmrwMEHKF.Z_axis_descr = "IQ (mg/stroke)";
                    shmrwMEHKF.X_axis_descr = "Engine speed (RPM)";
                    shmrwMEHKF.Y_axis_descr = "Injected Quantity (mg/stroke)";
                    shmrwMEHKF.Y_axis_correction = 0.01;
                    shmrwMEHKF.Correction = 0.01;
                    shmrwMEHKF.CodeBlock = DetermineCodeBlockByByAddress(shmrwMEHKF.Flash_start_address, newCodeBlocks);

                    newSymbols.Add(shmrwMEHKF);

                    offset2 = mrwMERKFAddress + 1;
                }*/

                else found = false;
            }
            return mrwMERKFfound;
        }

        public override void NameKnownMaps(byte[] allBytes, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            SymbolAxesTranslator st = new SymbolAxesTranslator();
            long boosttagetmaploc = 0;
            bool eb96CylHeadPresent = false;

            foreach (SymbolHelper sh in newSymbols)
            {
                //sh.X_axis_descr = st.TranslateAxisID(sh.X_axis_ID);
                //sh.Y_axis_descr = st.TranslateAxisID(sh.Y_axis_ID);
                if (sh.Length == 700) // 25*14
                {
                    sh.Category = "Detected maps";
                    sh.Subcategory = "Misc";
                    sh.Varname = "Launch control map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                    sh.Y_axis_correction = 0.156250;
                    //sh.Y_axis_correction = 0.000039;
                    sh.Correction = 0.01;
                    sh.X_axis_descr = "Engine speed (rpm)";
                    //sh.Y_axis_descr = "Ratio vehicle/engine speed";
                    sh.Y_axis_descr = "Approx. vehicle speed (km/h)";
                    //sh.Z_axis_descr = "Output percentage";
                    sh.Z_axis_descr = "IQ limit";
                    sh.YaxisUnits = "km/h";
                    sh.XaxisUnits = "rpm";
                }

                if (sh.Length == 570)
                {
                    if (sh.X_axis_ID / 256 == 0xC5 && sh.Y_axis_ID / 256 == 0xEC)
                    {

                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                        injDurCount--;
                        //if (injDurCount < 1) injDurCount = 1;

                        sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.023437;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        //sh.Y_axis_descr = "Airflow mg/stroke";
                        sh.Y_axis_descr = "Requested Quantity mg/stroke";
                        sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mg/st";
                    }
                    else if (sh.X_axis_ID / 256 == 0xC4 && sh.Y_axis_ID / 256 == 0xEA)
                    {

                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                        injDurCount--;
                        //if (injDurCount < 1) injDurCount = 1;

                        sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.023437;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        //sh.Y_axis_descr = "Airflow mg/stroke";
                        sh.Y_axis_descr = "Requested Quantity mg/stroke";

                        sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mg/st";
                    }
                    else if (sh.X_axis_ID / 256 == 0xC4 && sh.Y_axis_ID / 256 == 0xEC)
                    {

                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                        injDurCount--;
                        //if (injDurCount < 1) injDurCount = 1;

                        sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.023437;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        //sh.Y_axis_descr = "Airflow mg/stroke";
                        sh.Y_axis_descr = "Requested Quantity mg/stroke";

                        sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mg/st";
                    }
                }
                else if (sh.Length == 480)
                {
                    if (sh.X_axis_ID / 256 == 0xC5 && sh.Y_axis_ID / 256 == 0xEC)
                    {

                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                        injDurCount--;
                        //if (injDurCount < 1) injDurCount = 1;
                        //IAT, ECT or Fuel temp?

                        double tempRange = GetTemperatureDurRange(injDurCount - 1);
                        sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.023437;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.Y_axis_descr = "Requested Quantity mg/stroke";
                        sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mg/st";
                    }
                    else if (sh.X_axis_ID / 256 == 0xC4 && sh.Y_axis_ID / 256 == 0xEA)
                    {

                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                        injDurCount--;
                        //if (injDurCount < 1) injDurCount = 1;
                        //IAT, ECT or Fuel temp?

                        double tempRange = GetTemperatureDurRange(injDurCount - 1);
                        sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.023437;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.Y_axis_descr = "Requested Quantity mg/stroke";
                        sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mg/st";
                    }

                }
                else if (sh.Length == 448)
                {
                    if (sh.MapSelector.NumRepeats == 10)
                    {
                        // SOI maps detected
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Start of injection (SOI)", sh.CodeBlock, newSymbols, false);

                        //based on coolant temperature
                        double tempRange = GetTemperatureSOIRange(sh.MapSelector, injDurCount - 1);
                        sh.Varname = "Start of injection (SOI) " + tempRange.ToString() + " °C [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");

                        sh.Correction = -0.023437;
                        sh.Offset = 78;

                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "rpm";
                        sh.X_axis_correction = 0.01; // TODODONE : Check for x or y
                        sh.XaxisUnits = "mg/st";

                        sh.X_axis_descr = "IQ (mg/stroke)";
                        sh.Z_axis_descr = "Start position (degrees BTDC)";
                    }
                    else if (sh.X_axis_ID / 256 == 0xC5 && sh.Y_axis_ID / 256 == 0xEC)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                        injDurCount--;
                        //if (injDurCount < 1) injDurCount = 1;

                        sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.023437;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        //sh.Y_axis_descr = "Airflow mg/stroke";
                        sh.Y_axis_descr = "Requested Quantity mg/stroke";

                        sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mg/st";

                    }
                    else if (sh.X_axis_ID / 256 == 0xF9 && sh.Y_axis_ID / 256 == 0xEB)
                    {
                        if (sh.MapSelector.NumRepeats == 6)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel";
                            int hghtCorCount = GetMapNameCountForCodeBlock("Injection correction map (Height)", sh.CodeBlock, newSymbols, false);
                            hghtCorCount--;
                            //if (injDurCount < 1) injDurCount = 1;

                            sh.Varname = "Injection correction map (Height) " + hghtCorCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.X_axis_correction = 0.01;
                            sh.Correction = 0.023437;
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.X_axis_descr = "Requested Quantity mg/stroke";

                            sh.Z_axis_descr = "Duration (crankshaft degrees)";
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "mg/st";

                        }
                        else if (sh.MapSelector.NumRepeats == 5)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel";
                            int dynAdvCount = GetMapNameCountForCodeBlock("Injection dynamic advance map", sh.CodeBlock, newSymbols, false);
                            dynAdvCount--;
                            //if (injDurCount < 1) injDurCount = 1;

                            sh.Varname = "Injection dynamic advance map " + dynAdvCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.X_axis_correction = 0.01;
                            sh.Correction = 0.023437;
                            sh.Offset = -78;
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.X_axis_descr = "Requested Quantity mg/stroke";

                            sh.Z_axis_descr = "Duration (crankshaft degrees)";
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "mg/st";

                        }
                        else if (sh.MapSelector.NumRepeats == 4)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel";
                            int iatCorCount = GetMapNameCountForCodeBlock("Injection correction map (Air Temperature)", sh.CodeBlock, newSymbols, false);
                            iatCorCount--;
                            //if (injDurCount < 1) injDurCount = 1;
                            sh.Varname = "Injection correction map (Air Temperature) " + iatCorCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.X_axis_correction = 0.01;
                            sh.Correction = 0.023437;
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.X_axis_descr = "Requested Quantity mg/stroke";

                            sh.Z_axis_descr = "Duration (crankshaft degrees)";
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "mg/st";

                        }

                    }
                }
                else if (sh.Length == 416)
                {
                    string strAddrTest = sh.Flash_start_address.ToString("X8");
                    if (sh.X_axis_ID / 256 == 0xF9 && sh.Y_axis_ID / 256 == 0xDA)
                    {
                        // this is IQ by MAF limiter!
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        int smokeCount = GetMapNameCountForCodeBlock("Smoke limiter", sh.CodeBlock, newSymbols, false);
                        //sh.Varname = "Smoke limiter " + smokeCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Varname = "Smoke limiter [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        if (sh.MapSelector != null)
                        {
                            if (sh.MapSelector.MapIndexes != null)
                            {
                                if (sh.MapSelector.MapIndexes.Length > 1)
                                {
                                    if (!MapSelectorIndexEmpty(sh))
                                    {
                                        double tempRange = GetTemperatureSOIRange(sh.MapSelector, smokeCount - 1);
                                        sh.Varname = "Smoke limiter " + tempRange.ToString() + " °C [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                                    }
                                }
                            }
                        }
                        sh.Z_axis_descr = "Maximum IQ (mg)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.X_axis_descr = "Airflow mg/stroke";
                        sh.Correction = 0.01;
                        sh.X_axis_correction = 0.1;
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "mg/st";

                    }
                    else if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xDA)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        // if x axis = upto 3000 -> MAP limit, not MAF limit
                        if (GetMaxAxisValue(allBytes, sh, MapViewerEx.AxisIdent.Y_Axis) < 4000)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Limiters";
                            sh.Varname = "IQ by MAP limiter [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") +" " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                            sh.Correction = 0.01;
                            sh.X_axis_descr = "Boost pressure";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.Z_axis_descr = "Maximum IQ (mg)";
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "mbar";

                        }
                        else
                        {
                            int iqMAFLimCount = GetMapNameCountForCodeBlock("IQ by MAF limiter", sh.CodeBlock, newSymbols, false);
                            sh.Varname = "IQ by MAF limiter " + iqMAFLimCount.ToString() + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            //sh.Varname = "IQ by MAF limiter [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Z_axis_descr = "Maximum IQ (mg)";
                            sh.Correction = 0.01;
                            sh.X_axis_correction = 0.1;
                            sh.XaxisUnits = "mg/st";
                            sh.X_axis_descr = "Airflow mg/stroke";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                        }

                    }
                    else if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xEA)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Turbo";
                        sh.Varname = "N75 duty cycle [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Z_axis_descr = "Duty cycle %";
                        sh.Correction = -0.01;
                        sh.Offset = 100;
                        //sh.Correction = 0.01;
                        sh.X_axis_correction = 0.01;
                        sh.X_axis_descr = "IQ (mg/stroke)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "mg/st";
                    }
                    /*else if (strAddrTest.EndsWith("116"))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        int egrCount = GetMapNameCountForCodeBlock("EGR", sh.CodeBlock, newSymbols, false);
                        sh.Varname = "EGR " + egrCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Correction = 0.1;
                        sh.X_axis_correction = 0.01;
                        sh.Z_axis_descr = "Mass Air Flow (mg/stroke)";
                        sh.X_axis_descr = "IQ (mg/stroke)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "mg/st";
                    }*/
                    else if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID == 0xE9D4)
                    {
                        // x axis should start with 0!
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Turbo";
                        sh.Varname = "N75 duty cycle [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Z_axis_descr = "Duty cycle %";
                        sh.Correction = -0.01;
                        sh.Offset = 100;
                        //sh.Correction = 0.01;
                        sh.X_axis_correction = 0.01;
                        sh.X_axis_descr = "IQ (mg/stroke)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "mg/st";
                    }
                    else if ((sh.X_axis_ID / 256 == 0xEC) && (sh.Y_axis_ID / 256 == 0xC0 || sh.Y_axis_ID / 256 == 0xE9))
                    {
                        // x axis should start with 0!
                        if (allBytes[sh.Y_axis_address] == 0 && allBytes[sh.Y_axis_address + 1] == 0)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            int egrCount = GetMapNameCountForCodeBlock("EGR", sh.CodeBlock, newSymbols, false);
                            sh.Varname = "EGR " + egrCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Correction = 0.1;
                            sh.X_axis_correction = 0.01;
                            sh.Z_axis_descr = "Mass Air Flow (mg/stroke)";
                            sh.X_axis_descr = "IQ (mg/stroke)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "mg/st";
                        }
                    }
                    else if (sh.X_axis_ID / 256 == 0xEA && sh.Y_axis_ID / 256 == 0xE9)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Start of injection (SOI)", sh.CodeBlock, newSymbols, false);
                        //IAT, ECT or Fuel temp?
                        double tempRange = GetTemperatureSOIRange(sh.MapSelector, injDurCount - 1);
                        sh.Varname = "Start of injection (SOI) " + tempRange.ToString() + " °C [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                        sh.Correction = -0.023437;
                        sh.Offset = 78;

                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "rpm";
                        sh.X_axis_correction = 0.01; // TODODONE : Check for x or y
                        sh.XaxisUnits = "mg/st";

                        sh.Z_axis_descr = "Start position (degrees BTDC)";
                    }
                    else if (sh.X_axis_ID / 256 == 0xEA && sh.Y_axis_ID / 256 == 0xE8)
                    {
                        // EGR or N75
                        if (allBytes[sh.X_axis_address] == 0 && allBytes[sh.X_axis_address + 1] == 0)
                        {
                            // EGR
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            int egrCount = GetMapNameCountForCodeBlock("EGR", sh.CodeBlock, newSymbols, false);
                            sh.Varname = "EGR " + egrCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Correction = 0.1;
                            sh.X_axis_correction = 0.01;
                            sh.Z_axis_descr = "Mass Air Flow (mg/stroke)";
                            sh.X_axis_descr = "IQ (mg/stroke)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "mg/st";

                        }
                        else
                        {
                            //N75
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Turbo";
                            sh.Varname = "N75 duty cycle [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Z_axis_descr = "Duty cycle %";
                            sh.Correction = -0.01;
                            sh.Offset = 100;
                            //sh.Correction = 0.01;
                            sh.X_axis_correction = 0.01;
                            sh.X_axis_descr = "IQ (mg/stroke)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "mg/st";

                        }
                    }
                    /* else if ((sh.X_axis_ID / 256 == 0xEA) && (sh.Y_axis_ID / 256 == 0xE8))
                     {
                         // x axis should start with 0!
                         if (allBytes[sh.Y_axis_address] == 0 && allBytes[sh.Y_axis_address + 1] == 0)
                         {
                             sh.Category = "Detected maps";
                             sh.Subcategory = "Misc";
                             int egrCount = GetMapNameCountForCodeBlock("EGR", sh.CodeBlock, newSymbols, false);
                             sh.Varname = "EGR " + egrCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                             sh.Correction = 0.1;
                             sh.X_axis_correction = 0.01;
                             sh.Z_axis_descr = "Mass Air Flow (mg/stroke)";
                             sh.X_axis_descr = "IQ (mg/stroke)";
                             sh.Y_axis_descr = "Engine speed (rpm)";
                             sh.YaxisUnits = "rpm";
                             sh.XaxisUnits = "mg/st";
                         }
                     }*/
                }
                else if (sh.Length == 390)
                {
                    // 15x12 = inj dur limiter on R3 files
                    if (sh.X_axis_length == 13 && sh.Y_axis_length == 15)
                    {
                        /* sh.Category = "Detected maps";
                         sh.Subcategory = "Limiters";
                         sh.Varname = "Injection duration limiter B [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                         sh.Correction = 0.023438;
                         sh.Y_axis_correction = 0.01;
                         sh.Y_axis_descr = "IQ (mg/stroke)";
                         sh.Z_axis_descr = "Max. degrees";
                         sh.X_axis_descr = "Engine speed (rpm)";
                         sh.XaxisUnits = "rpm";
                         sh.YaxisUnits = "mg/st";*/
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                        injDurCount--;
                        //if (injDurCount < 1) injDurCount = 1;

                        sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.023437;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        //sh.Y_axis_descr = "Airflow mg/stroke";
                        sh.Y_axis_descr = "Requested Quantity mg/stroke";

                        sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mg/st";
                    }

                    else if ((sh.X_axis_ID / 256 == 0xEC) && (sh.Y_axis_ID / 256 == 0xC0))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        int egrCount = GetMapNameCountForCodeBlock("EGR", sh.CodeBlock, newSymbols, false);
                        sh.Varname = "EGR " + egrCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Correction = 0.1;
                        sh.X_axis_correction = 0.01;
                        sh.Z_axis_descr = "Mass Air Flow (mg/stroke)";
                        sh.X_axis_descr = "IQ (mg/stroke)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "mg/st";
                    }

                }
                else if (sh.Length == 384)
                {
                    if (sh.X_axis_length == 12 && sh.Y_axis_length == 16)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Inverse driver wish [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Correction = 0.01;
                        sh.X_axis_correction = 0.01;
                        sh.Z_axis_descr = "Throttle  position";
                        sh.X_axis_descr = "IQ (mg/stroke)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        //sh.Z_axis_descr = "Requested IQ (mg)";
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "mg/st";
                    }
                    else if (sh.X_axis_length == 16 && sh.Y_axis_length == 12)
                    {
                        if ((sh.X_axis_ID / 256 == 0xEA) && (sh.Y_axis_ID / 256 == 0xDA))
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Limiters";
                            int smokeCount = GetMapNameCountForCodeBlock("Smoke limiter", sh.CodeBlock, newSymbols, false);
                            //sh.Varname = "Smoke limiter " + smokeCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Varname = "Smoke limiter [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            if (sh.MapSelector != null)
                            {
                                if (sh.MapSelector.MapIndexes != null)
                                {
                                    if (sh.MapSelector.MapIndexes.Length > 1)
                                    {
                                        if (!MapSelectorIndexEmpty(sh))
                                        {
                                            double tempRange = GetTemperatureSOIRange(sh.MapSelector, smokeCount - 1);
                                            sh.Varname = "Smoke limiter " + tempRange.ToString() + " °C [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                                        }
                                    }
                                }
                            }
                            sh.Z_axis_descr = "Maximum IQ (mg)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.X_axis_descr = "Airflow mg/stroke";
                            sh.Correction = 0.01;
                            sh.X_axis_correction = 0.1;
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "mg/st";
                        }
                        else if ((sh.X_axis_ID / 256 == 0xEC) && (sh.Y_axis_ID / 256 == 0xC0))
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            int egrCount = GetMapNameCountForCodeBlock("EGR", sh.CodeBlock, newSymbols, false);
                            sh.Varname = "EGR " + egrCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Correction = 0.1;
                            sh.X_axis_correction = 0.01;
                            sh.Z_axis_descr = "Mass Air Flow (mg/stroke)";
                            sh.X_axis_descr = "IQ (mg/stroke)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "mg/st";
                        }
                    }
                }
                else if (sh.Length == 360)
                {
                    // 15x12 = inj dur limiter on R3 files
                    if (sh.X_axis_length == 12 && sh.Y_axis_length == 15)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                        injDurCount--;
                        //if (injDurCount < 1) injDurCount = 1;

                        sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.023437;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        //sh.Y_axis_descr = "Airflow mg/stroke";
                        sh.Y_axis_descr = "Requested Quantity mg/stroke";

                        sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mg/st";
                    }

                }
                else if (sh.Length == 352)
                {
                    if (sh.X_axis_length == 16 && sh.Y_axis_length == 11)
                    {
                        if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xC0)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            int egrCount = GetMapNameCountForCodeBlock("EGR", sh.CodeBlock, newSymbols, false);
                            sh.Varname = "EGR " + egrCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Correction = 0.1;
                            sh.X_axis_correction = 0.01;
                            sh.Z_axis_descr = "Mass Air Flow (mg/stroke)";
                            sh.X_axis_descr = "IQ (mg/stroke)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "mg/st";
                        }
                        else if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xEA)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Turbo";
                            sh.Varname = "N75 duty cycle [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Z_axis_descr = "Duty cycle %";
                            sh.Correction = -0.01;
                            sh.Offset = 100;
                            sh.X_axis_correction = 0.01;
                            sh.X_axis_descr = "IQ (mg/stroke)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "mg/st";
                        }
                    }
                }
                else if (sh.Length == 320)
                {
                    if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xC0)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Turbo";
                        sh.Varname = "Boost target map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                        sh.X_axis_correction = 0.01;
                        sh.X_axis_descr = "IQ (mg/stroke)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.Z_axis_descr = "Boost target (mbar)";
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "mg/st";
                        boosttagetmaploc = sh.X_axis_address;
                    }
                    else if (sh.X_axis_ID / 256 == 0xEA && sh.Y_axis_ID / 256 == 0xC0)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Turbo";
                        sh.Varname = "Boost target map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                        sh.X_axis_correction = 0.01;
                        sh.X_axis_descr = "IQ (mg/stroke)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.Z_axis_descr = "Boost target (mbar)";
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "mg/st";
                        boosttagetmaploc = sh.X_axis_address;
                    }
                    else if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xDA)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "IQ by MAP limiter [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") +" " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                        sh.Correction = 0.01;
                        sh.X_axis_descr = "Boost pressure";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.Z_axis_descr = "Maximum IQ (mg)";
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "mbar";
                    }
                    else
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "MAF airmass correction by temperature [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.X_axis_descr = "Temperature (celsius)";
                        sh.Y_axis_descr = "Airflow (mg/stroke)";
                        sh.Z_axis_descr = "Corrected Airflow (mg/stroke)";
                        sh.Y_axis_correction = 0.1;
                        sh.X_axis_correction = 0.1;
                        sh.X_axis_offset = -273.1;
                        sh.Correction = 0.1;
                        sh.XaxisUnits = "°C";
                        sh.YaxisUnits = "mgst";

                    }

                }
                else if (sh.Length == 308)
                {
                    sh.Category = "Detected maps";
                    sh.Subcategory = "Limiters";
                    //sh.Varname = "Boost limiter (temperature) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                    sh.Varname = "SOI limiter (temperature) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                    sh.Correction = -0.023437;
                    sh.Offset = 78;
                    sh.Y_axis_descr = "Engine speed (rpm)";
                    sh.X_axis_descr = "Temperature"; //IAT, ECT or Fuel temp?
                    sh.X_axis_correction = 0.1;
                    sh.X_axis_offset = -273.1;
                    sh.Z_axis_descr = "SOI limit (degrees)";
                    sh.YaxisUnits = "rpm";
                    sh.XaxisUnits = "°C";
                }
                else if (sh.Length == 286)
                {
                    if (sh.X_axis_length == 0x0d && sh.Y_axis_length == 0x0b)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Driver wish [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Correction = 0.01;
                        sh.X_axis_correction = 0.01;
                        sh.X_axis_descr = "Throttle  position";
                        sh.Z_axis_descr = "Requested IQ (mg)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "TPS %";
                    }
                }
                else if (sh.Length == 280) // boost target can be 10x14 as well in Seat maps
                {
                    if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xC0)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Turbo";
                        sh.Varname = "Boost target map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "] " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                        sh.X_axis_correction = 0.01;
                        sh.X_axis_descr = "IQ (mg/stroke)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.Z_axis_descr = "Boost target (mbar)";
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "mg/st";
                    }
                }
                else if (sh.Length == 260) // EXPERIMENTAL
                {
                    if (sh.X_axis_ID / 256 == 0xC5 && sh.Y_axis_ID / 256 == 0xEC)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                        injDurCount--;
                        //if (injDurCount < 1) injDurCount = 1;

                        sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.023437;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        //sh.Y_axis_descr = "Airflow mg/stroke";
                        sh.Y_axis_descr = "Requested Quantity mg/stroke";

                        sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mg/st";

                    }
                }
                else if (sh.Length == 256)
                {
                    sh.Category = "Detected maps";
                    sh.Subcategory = "Misc";
                    sh.Varname = "Driver wish [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                    sh.Correction = 0.01;
                    sh.X_axis_correction = 0.01;
                    sh.X_axis_descr = "Throttle  position";
                    sh.Z_axis_descr = "Requested IQ (mg)";
                    sh.Y_axis_descr = "Engine speed (rpm)";
                    sh.YaxisUnits = "rpm";
                    sh.XaxisUnits = "TPS %";

                }
                else if (sh.Length == 240)
                {
                    if (sh.X_axis_length == 12 && sh.Y_axis_length == 10)
                    {
                        if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xC0)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Driver wish [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Correction = 0.01;
                            sh.X_axis_correction = 0.01;
                            sh.X_axis_descr = "Throttle  position";
                            sh.Z_axis_descr = "Requested IQ (mg)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "TPS %";
                        }
                    }

                }
                else if (sh.Length == 220) // EXPERIMENTAL
                {
                    if (sh.X_axis_ID / 256 == 0xC5 && sh.Y_axis_ID / 256 == 0xEC)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                        injDurCount--;
                        //if (injDurCount < 1) injDurCount = 1;

                        sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.023437;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        //sh.Y_axis_descr = "Airflow mg/stroke";
                        sh.Y_axis_descr = "Requested Quantity mg/stroke";

                        sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mg/st";

                    }
                    else if (sh.X_axis_ID / 256 == 0xE9 && sh.Y_axis_ID / 256 == 0xE9)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Diagnostics";
                        sh.Varname = "Measurement Blocks 190-199 values [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Offset = -3840;
                        sh.X_axis_descr = "Measurement block number";
                        sh.Y_axis_descr = "Diagnostic Value Slot number";
                        sh.Z_axis_descr = "Diagnostic Value Type (ID) (-3585 = Unused)";
                        //sh.XaxisUnits = "rpm";
                        //sh.YaxisUnits = "mg/st";
                    }
                }
                else if (sh.Length == 216)
                {
                    if (sh.X_axis_length == 12 && sh.Y_axis_length == 9)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Driver wish [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Correction = 0.01;
                        sh.X_axis_correction = 0.01;
                        sh.X_axis_descr = "Throttle  position";
                        sh.Z_axis_descr = "Requested IQ (mg)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "TPS %";
                    }

                }
                else if (sh.Length == 200)
                {
                    if (sh.X_axis_ID / 256 == 0xC0 && sh.Y_axis_ID / 256 == 0xEC)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "Boost limit map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                        //   sh.Correction = 0.01;
                        //sh.X_axis_correction = 0.01;
                        sh.Y_axis_descr = "Atmospheric pressure (mbar)";
                        sh.Z_axis_descr = "Maximum boost pressure (mbar)";
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mbar";
                    }
                    else if (sh.X_axis_ID / 256 == 0xC0 && sh.Y_axis_ID / 256 == 0xEA)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "Boost limit map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                        //   sh.Correction = 0.01;
                        //sh.X_axis_correction = 0.01;
                        sh.Y_axis_descr = "Atmospheric pressure (mbar)";
                        sh.Z_axis_descr = "Maximum boost pressure (mbar)";
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mbar";
                    }
                    else if (sh.X_axis_ID / 256 == 0xC5 && sh.Y_axis_ID / 256 == 0xEC)
                    {
                        //if (!MapContainsNegativeValues(allBytes, sh))
                        if (GetMaxAxisValue(allBytes, sh, MapViewerEx.AxisIdent.X_Axis) > 3500) // was 5000
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel"; // was Limiters
                            // was limiter
                            int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                            injDurCount--;

                            sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                            sh.Correction = 0.023438;
                            sh.Y_axis_correction = 0.01;
                            sh.Y_axis_descr = "IQ (mg/stroke)";
                            //sh.Z_axis_descr = "Max. degrees";
                            sh.Z_axis_descr = "Duration (crankshaft degrees)";

                            sh.X_axis_descr = "Engine speed (rpm)";
                            sh.XaxisUnits = "rpm";
                            sh.YaxisUnits = "mg/st";

                        }
                        else
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel";
                            int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                            injDurCount--;
                            //if (injDurCount < 1) injDurCount = 1;
                            sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_correction = 0.01;
                            sh.Correction = 0.023437;
                            sh.X_axis_descr = "Engine speed (rpm)";
                            //sh.Y_axis_descr = "Airflow mg/stroke";
                            sh.Y_axis_descr = "Requested Quantity mg/stroke";

                            sh.Z_axis_descr = "Duration (crankshaft degrees)";
                            sh.XaxisUnits = "rpm";
                            sh.YaxisUnits = "mg/st";

                        }
                    }
                    else if (sh.X_axis_ID / 256 == 0xC4 && sh.Y_axis_ID / 256 == 0xEA)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                        injDurCount--;
                        //if (injDurCount < 1) injDurCount = 1;
                        sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.023437;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        //sh.Y_axis_descr = "Airflow mg/stroke";
                        sh.Y_axis_descr = "Requested Quantity mg/stroke";

                        sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mg/st";
                    }
                    else if (sh.X_axis_ID / 256 == 0xC4 && sh.Y_axis_ID / 256 == 0xEC)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                        injDurCount--;
                        //if (injDurCount < 1) injDurCount = 1;
                        sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.023437;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        //sh.Y_axis_descr = "Airflow mg/stroke";
                        sh.Y_axis_descr = "Requested Quantity mg/stroke";

                        sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mg/st";
                    }
                    else if ((sh.X_axis_ID / 256 == 0xEC) && (sh.Y_axis_ID / 256 == 0xC2 || sh.Y_axis_ID / 256 == 0xC1))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "Limit of overboost protection (zmwVEPLSKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.X_axis_correction = 0.01;
                        sh.X_axis_descr = "VNT Actuator Duty Cycle (%)";
                        sh.Y_axis_descr = "Engine speed (rpm)";

                        sh.Z_axis_descr = "Pressure limit (mbar)";
                        sh.XaxisUnits = "%";
                        sh.YaxisUnits = "rpm";
                    }
                    else if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xEC)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "Overboost debouncing limit (zmwVETSuKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Correction = 10;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.Y_axis_descr = "Pressure of the boost overrrun (mbar)";

                        sh.Z_axis_descr = "Turbo debouncing time (ms)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mbar";
                    }
                    else if ((sh.X_axis_ID == 0xC1AE || sh.X_axis_ID == 0xC178 || sh.X_axis_ID == 0xC1BE) && (sh.Y_axis_ID / 256 == 0xEC))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "Fuel Temperature overheating protection map (mrwBKT_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_offset = -273.1;
                        sh.Y_axis_correction = 0.1;
                        sh.Correction = 0.0001;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.Y_axis_descr = "Fuel temperature (°C)";

                        //sh.Z_axis_descr = "";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "°C";
                    }
                    else if ((sh.X_axis_ID == 0xC1CE || sh.X_axis_ID == 0xC198 || sh.X_axis_ID == 0xC1DE) && (sh.Y_axis_ID / 256 == 0xEC))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "Fuel tank inlet temp overheat protection map (mrwBTT_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_offset = -273.1;
                        sh.Y_axis_correction = 0.1;
                        sh.Correction = 0.0001;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.Y_axis_descr = "Fuel temperature (°C)";

                        //sh.Z_axis_descr = "";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "°C";
                    }
                    else if (sh.X_axis_ID / 256 == 0xC2 && sh.Y_axis_ID / 256 == 0xC1)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Desired speed message to transmission via CAN (mrwBCV_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_correction = 0.01;
                        sh.X_axis_correction = 0.0001;
                        //sh.Correction = 0.0001;
                        sh.X_axis_descr = "none";
                        sh.Y_axis_descr = "Speed (km/h)";

                        //sh.Z_axis_descr = "";
                        sh.XaxisUnits = "-";
                        sh.YaxisUnits = "KMH";
                    }
                    else if ((sh.X_axis_ID / 256 == 0xEC) && (sh.Y_axis_ID / 256 == 0xC0 || sh.Y_axis_ID / 256 == 0xE9))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "EGR PWM Basic Map (arwSTTVKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        //sh.Y_axis_correction = 0.01;
                        sh.X_axis_correction = 0.01;
                        sh.Correction = 0.01;
                        sh.X_axis_descr = "Injected Quantity (mg/stroke)";
                        sh.Y_axis_descr = "Engine speed (rpm)";

                        sh.Z_axis_descr = "EGR PWM (%)";
                        sh.XaxisUnits = "mgst";
                        sh.YaxisUnits = "RPM";
                    }
                    else if ((sh.X_axis_ID / 256 == 0xEC) && (sh.Y_axis_ID / 256 == 0xF9))
                    {
                        int AG4count = GetMapNameCountForCodeBlock("mrw Map - ", sh.CodeBlock, newSymbols, false);
                        AG4count--;
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.XaxisUnits = "RPM";
                        sh.YaxisUnits = "mg/st";;
                        sh.X_axis_descr = "Engine speed (RPM)";
                        sh.Y_axis_descr = "Injected Quantity (mg/stroke)";
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.01;
                        if (AG4count % 3 == 0)
                        {
                            sh.Varname = "mrw Map - AG4 Transmission downshift map (mrwM_ER_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "mrwM_ER_KF";
                            sh.Z_axis_descr = "IQ (mg/stroke)";
                        }
                        if (AG4count % 3 == 1)
                        {
                            sh.Varname = "mrw Map - AG4 Transmission upshift map (mrwM_EH_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "mrwM_EH_KF";
                            sh.Z_axis_descr = "IQ (mg/stroke)";
                        }
                        if (AG4count % 3 == 2)
                        {
                            sh.Varname = "mrw Map -  Injected Fuel Volume Correction at 100°C (mrwKTB_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "mrwKTB_KF";
                            sh.Z_axis_descr = "IQ correction (mg/st)";
                        }
                    }
                    /*else
                    {
                        sh.Category = "Debug detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                    }*/
                }
                else if (sh.Length == 198) // EXPERIMENTAL
                {
                    if (sh.X_axis_ID / 256 == 0xC5 && sh.Y_axis_ID / 256 == 0xEC)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                        injDurCount--;
                        //if (injDurCount < 1) injDurCount = 1;

                        sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.023437;
                        sh.X_axis_descr = "Engine speed (rpm)";
                        //sh.Y_axis_descr = "Airflow mg/stroke";
                        sh.Y_axis_descr = "Requested Quantity mg/stroke";

                        sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mg/st";

                    }
                }
                else if (sh.Length == 192)
                {
                    if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xC0)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Driver wish [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Correction = 0.01;
                        sh.X_axis_correction = 0.01;
                        sh.X_axis_descr = "Throttle  position";
                        sh.Z_axis_descr = "Requested IQ (mg)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "TPS %";

                    }
                    if (sh.X_axis_ID / 256 == 0xC2 && sh.Y_axis_ID / 256 == 0xC0)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Torque moment driving behavior map (mrwFGFVHKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Correction = 0.01;
                        sh.X_axis_correction = 0.01;
                        sh.Y_axis_correction = 0.01;
                        sh.X_axis_descr = "Throttle  position";
                        sh.Z_axis_descr = "Torque (nm)";
                        sh.Y_axis_descr = "Vehicle speed (km/h)";
                        sh.YaxisUnits = "KMH";
                        sh.XaxisUnits = "TPS %";

                    }
                    /*else
                    {
                        sh.Category = "Debug detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                    }*/
                }
                else if (sh.Length == 180)
                {
                    if (sh.X_axis_length == 9 && sh.Y_axis_length == 10)
                    {
                        if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xC1)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel";
                            int sIQCount = GetMapNameCountForCodeBlock("Start IQ ", sh.CodeBlock, newSymbols, false);
                            sh.Varname = "Start IQ (" + sIQCount.ToString() + ") [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Correction = 0.01;
                            sh.X_axis_descr = "CT (celcius)";
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.Z_axis_descr = "Requested IQ (mg)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "degC";
                        }
                        else if (sh.X_axis_ID / 256 == 0xC0 && sh.Y_axis_ID / 256 == 0xEC)
                        {
                            // atm boost limit R3 file versions
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Limiters";
                            sh.Varname = "Boost limit map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                            //   sh.Correction = 0.01;
                            //sh.X_axis_correction = 0.01;
                            sh.Y_axis_descr = "Atmospheric pressure (mbar)";
                            sh.Z_axis_descr = "Maximum boost pressure (mbar)";
                            sh.X_axis_descr = "Engine speed (rpm)";
                            sh.XaxisUnits = "rpm";
                            sh.YaxisUnits = "mbar";
                        }
                        else if (sh.X_axis_ID / 256 == 0xC5 && sh.Y_axis_ID / 256 == 0xEC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel";
                            int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                            injDurCount--;
                            //if (injDurCount < 1) injDurCount = 1;
                            sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_correction = 0.01;
                            sh.Correction = 0.023437;
                            sh.X_axis_descr = "Engine speed (rpm)";
                            //sh.Y_axis_descr = "Airflow mg/stroke";
                            sh.Y_axis_descr = "Requested Quantity mg/stroke";

                            sh.Z_axis_descr = "Duration (crankshaft degrees)";
                            sh.XaxisUnits = "rpm";
                            sh.YaxisUnits = "mg/st";
                        }
                        else if (sh.X_axis_ID / 256 == 0xC4 && sh.Y_axis_ID / 256 == 0xEA)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel";
                            int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                            injDurCount--;
                            //if (injDurCount < 1) injDurCount = 1;
                            sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_correction = 0.01;
                            sh.Correction = 0.023437;
                            sh.X_axis_descr = "Engine speed (rpm)";
                            //sh.Y_axis_descr = "Airflow mg/stroke";
                            sh.Y_axis_descr = "Requested Quantity mg/stroke";

                            sh.Z_axis_descr = "Duration (crankshaft degrees)";
                            sh.XaxisUnits = "rpm";
                            sh.YaxisUnits = "mg/st";
                        }
                    }
                    else if (sh.X_axis_length == 10 && sh.Y_axis_length == 9)
                    {
                        if (sh.X_axis_ID / 256 == 0xC5 && sh.Y_axis_ID / 256 == 0xEC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel";
                            int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                            injDurCount--;
                            //if (injDurCount < 1) injDurCount = 1;
                            sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_correction = 0.01;
                            sh.Correction = 0.023437;
                            sh.X_axis_descr = "Engine speed (rpm)";
                            //sh.Y_axis_descr = "Airflow mg/stroke";
                            sh.Y_axis_descr = "Requested Quantity mg/stroke";

                            sh.Z_axis_descr = "Duration (crankshaft degrees)";
                            sh.XaxisUnits = "rpm";
                            sh.YaxisUnits = "mg/st";
                        }
                    }
                }
                else if (sh.Length == 176)
                {
                    if (sh.X_axis_length == 8 && sh.Y_axis_length == 11)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Calculation of specific consuption (for CAN) (mrwKFVB_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.X_axis_correction = 0.01;
                        sh.XaxisUnits = "mg/st";
                        sh.X_axis_descr = "Requested quantity (mg/stroke)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "rpm";
                        sh.Correction = 0.0002;
                        sh.Z_axis_descr = "IQ to Nm ratio (mg/stroke/Nm)";
                    }
                    /*else
                    {
                        sh.Category = "Debug detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                    }*/
                }
                else if (sh.Length == 162)
                {
                    if (sh.X_axis_length == 9 && sh.Y_axis_length == 9)
                    {
                        if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xC1)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel";
                            int sIQCount = GetMapNameCountForCodeBlock("Start IQ ", sh.CodeBlock, newSymbols, false);
                            sh.Varname = "Start IQ (" + sIQCount.ToString() + ") [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Correction = 0.01;
                            sh.X_axis_descr = "CT (celcius)";
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.Z_axis_descr = "Requested IQ (mg)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "degC";
                        }
                    }
                    /*else
                    {
                        sh.Category = "Debug detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                    }*/
                }
                else if (sh.Length == 160)
                {
                    if (sh.X_axis_length == 8 && sh.Y_axis_length == 10)
                    {
                        if (sh.X_axis_ID / 256 == 0xC5 && sh.Y_axis_ID / 256 == 0xEC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel";
                            int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                            injDurCount--;
                            sh.Varname = "Injector duration " + injDurCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_correction = 0.01;
                            sh.Correction = 0.023437;
                            sh.X_axis_descr = "Engine speed (rpm)";
                            //sh.Y_axis_descr = "Airflow mg/stroke";
                            sh.Y_axis_descr = "Requested Quantity mg/stroke";

                            sh.Z_axis_descr = "Duration (crankshaft degrees)";
                            sh.XaxisUnits = "rpm";
                            sh.YaxisUnits = "mg/st";
                        }
                    }
                    else if (sh.X_axis_length == 10 && sh.Y_axis_length == 8)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Fuel";
                        sh.Varname = "BIP SOI Correction Map (zmwBPKorKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.X_axis_correction = 0.023437;
                        sh.X_axis_offset = -78;
                        sh.Correction = 0.00390625;
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.X_axis_descr = "Crankshaft degrees";
                        sh.Z_axis_descr = "BIP calculation";

                        //sh.Z_axis_descr = "Duration (crankshaft degrees)";
                        sh.YaxisUnits = "rpm";
                        sh.XaxisUnits = "°BTDC";
                    }
                }
                else if (sh.Length == 144)
                {
                    if (sh.X_axis_length == 9 && sh.Y_axis_length == 8)
                    {
                        if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xC0)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel";
                            sh.Varname = "Fuel volume correction map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "zmwMKOR_KF";
                            sh.Z_axis_descr = "IQ correction per 100K";
                            sh.Correction = 0.002441;
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.X_axis_correction = 0.01;
                            sh.X_axis_descr = "IQ (mg/stroke)";
                        }
                    }
                    else if (sh.X_axis_length == 8 && sh.Y_axis_length == 9)
                    {
                        if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xC1)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel";
                            int sIQCount = GetMapNameCountForCodeBlock("Start IQ ", sh.CodeBlock, newSymbols, false);
                            sh.Varname = "Start IQ (" + sIQCount.ToString() + ") [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.X_axis_descr = "CT (celcius)";
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.Z_axis_descr = "Requested IQ (mg)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.Correction = 0.01;
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "degC";
                        }
                    }
                    else if (sh.X_axis_length == 3 && sh.Y_axis_length == 24)
                    {
                        // Tq Lim
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "Torque limiter [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Z_axis_descr = "Maximum IQ (mg)";
                        sh.Y_axis_descr = "Atm. pressure (mbar)";
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.Correction = 0.01;
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mbar";
                    }
                    /*else
                    {
                        sh.Category = "Debug detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                    }*/
                }

                else if (sh.Length == 128)
                {
                    if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xC1)
                    {
                        // check for valid axis data on temp data
                        if (IsValidTemperatureAxis(allBytes, sh, MapViewerEx.AxisIdent.Y_Axis))
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Limiters";
                            int maflimTempCount = GetMapNameCountForCodeBlock("MAF correction by temperature", sh.CodeBlock, newSymbols, false);
                            maflimTempCount--;
                            sh.Varname = "MAF correction by temperature " + maflimTempCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Z_axis_descr = "Limit";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.X_axis_descr = "Intake air temperature"; //IAT, ECT or Fuel temp?
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.Correction = 0.01;
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "°C";
                        }

                    }
                    else if (sh.X_axis_ID / 256 == 0xEA && sh.Y_axis_ID / 256 == 0xC1)
                    {
                        // check for valid axis data on temp data
                        if (IsValidTemperatureAxis(allBytes, sh, MapViewerEx.AxisIdent.Y_Axis))
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Limiters";
                            int maflimTempCount = GetMapNameCountForCodeBlock("MAF correction by temperature", sh.CodeBlock, newSymbols, false);
                            maflimTempCount--;
                            sh.Varname = "MAF correction by temperature " + maflimTempCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Z_axis_descr = "Limit";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.X_axis_descr = "Intake air temperature"; //IAT, ECT or Fuel temp?
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.Correction = 0.01;
                            sh.YaxisUnits = "rpm";
                            sh.XaxisUnits = "°C";
                        }

                    }
                    else if ((sh.X_axis_ID / 256 == 0xEA || sh.X_axis_ID / 256 == 0xEB) && sh.Y_axis_ID / 256 == 0xC0)
                    {
                        if (IsValidTemperatureAxis(allBytes, sh, MapViewerEx.AxisIdent.X_Axis))
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Limiters";
                            sh.Varname = "Boost correction by water temperature (ldwTW_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "ldwTW_KF";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Y_axis_descr = "Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.X_axis_correction = 0.01;
                            sh.Correction = 0.01;
                            sh.XaxisUnits = "mg/st";
                            sh.X_axis_descr = "IQ (mg/stroke)";
                            sh.Z_axis_descr = "Limitation of Boost (in %)";
                        }
                        else // EXPERIMENTAL
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Limiters";
                            sh.Varname = "Expected fuel temperature (zmwMKBT_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "zmwMKBT_KF";
                            sh.Correction = 0.1;
                            sh.Offset = -273.1;
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.X_axis_correction = 0.01;
                            sh.XaxisUnits = "mg/st";
                            sh.X_axis_descr = "IQ (mg/stroke)";
                            sh.Z_axis_descr = "Fuel temperature °C";


                        }
                    }
                    else if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xC0) // EXPERIMENTAL
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "Expected fuel temperature (zmwMKBT_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "zmwMKBT_KF";
                        sh.Correction = 0.1;
                        sh.Offset = -273.1;
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "rpm";
                        sh.X_axis_correction = 0.01;
                        sh.XaxisUnits = "mg/st";
                        sh.X_axis_descr = "IQ (mg/stroke)";
                        sh.Z_axis_descr = "Fuel temperature °C";


                    }
                    else if (sh.X_axis_ID / 256 == 0xC5 && sh.Y_axis_ID / 256 == 0xC1)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        int sodSwellCount = GetMapNameCountForCodeBlock("Start of delivery turn swell map", sh.CodeBlock, newSymbols, false);
                        sodSwellCount--;
                        sh.Varname = "Start of delivery turn swell map " + sodSwellCount.ToString("D2") + " (fnwUMDR_KF / fnwUMRMEKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "fnwUMDR_KF / fnwUMRMEKF";
                        sh.Y_axis_correction = 0.1;
                        sh.Y_axis_offset = -273.1;
                        sh.Y_axis_descr = "Temperature (°C)";
                        sh.YaxisUnits = "°C";
                        //sh.X_axis_correction = 0.01;
                        //sh.Correction = 0.01;
                        sh.XaxisUnits = "mbar";
                        sh.X_axis_descr = "Ambient pressure (mBar)";
                        sh.Z_axis_descr = "Number of engine rotations";
                    }
                    else if (sh.X_axis_ID / 256 == 0xC5 && sh.Y_axis_ID / 256 == 0xC0)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        //int sodSwellCount = GetMapNameCountForCodeBlock("Start of delivery turn swell map", sh.CodeBlock, newSymbols, false);
                        //sodSwellCount--;
                        sh.Varname = "Start of delivery ATM pressure correction map (fnwSWSN_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "fnwSWSN_KF";
                        sh.Y_axis_correction = 0.1;
                        sh.Y_axis_offset = -273.1;
                        sh.Y_axis_descr = "Temperature (°C)";
                        sh.YaxisUnits = "°C";
                        //sh.X_axis_correction = 0.01;
                        sh.Correction = 0.0234375;
                        sh.XaxisUnits = "mbar";
                        sh.X_axis_descr = "Ambient pressure (mBar)";
                        sh.Z_axis_descr = "Crankshaft Degrees";
                    }
                    else if (sh.X_axis_ID / 256 == 0xF9 && sh.Y_axis_ID / 256 == 0xC1)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Engine Oil thermal load map (siwOEL_tKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "siwOEL_tKF";
                        sh.X_axis_correction = 0.1;
                        sh.X_axis_offset = -273.1;
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "RPM";
                        //sh.X_axis_correction = 0.01;
                        //sh.Correction = 0.1;
                        sh.XaxisUnits = "°C";
                        sh.X_axis_descr = "Engine Oil Temperature (°C)";
                        sh.Z_axis_descr = "Engine Oil thermal load index (counts into variable oil change interval)";
                    }
                    else if ((sh.X_axis_ID == 0xC1B6 && sh.Y_axis_ID == 0xC19E) || (sh.X_axis_ID == 0xC1C6 && sh.Y_axis_ID == 0xC1AE))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Radiator fan-caster correction map (kuwNLKORKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "kuwNLKORKF";
                        sh.X_axis_correction = 0.1;
                        sh.X_axis_offset = -273.1;
                        sh.Y_axis_correction = 0.1;
                        sh.Y_axis_offset = -273.1;
                        sh.Y_axis_descr = "Top of cylinder head Temperature (°C)";
                        sh.YaxisUnits = "°C";
                        //sh.X_axis_correction = 0.01;
                        //sh.Correction = 0.1;
                        sh.XaxisUnits = "°C";
                        sh.X_axis_descr = "Ambient Intake Temperature (°C)";
                        sh.Z_axis_descr = "Correction (s)";
                    }
                    else if (sh.X_axis_ID == 0xC180 && sh.Y_axis_ID == 0xC16A)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        int rfcCount = GetMapNameCountForCodeBlock("Radiator fan-caster correction map", sh.CodeBlock, newSymbols, false);
                        rfcCount--;
                        if (rfcCount % 2 == 1)
                        {
                            sh.Varname = "Radiator fan-caster correction map (kuwNLKORKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "kuwNLKORKF";
                            sh.Z_axis_descr = "Correction (s)";
                        }
                        if (rfcCount % 2 == 0)
                        {
                            sh.Varname = "Radiator fan-caster correction map 2 (kuwNLKORK2) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "kuwNLKORK2";
                            sh.Z_axis_descr = "Correction (s)";
                        }
                        sh.X_axis_correction = 0.1;
                        sh.X_axis_offset = -273.1;
                        sh.Y_axis_correction = 0.1;
                        sh.Y_axis_offset = -273.1;
                        sh.Y_axis_descr = "Top of cylinder head Temperature (°C)";
                        sh.YaxisUnits = "°C";
                        //sh.X_axis_correction = 0.01;
                        //sh.Correction = 0.1;
                        sh.XaxisUnits = "°C";
                        sh.X_axis_descr = "Ambient Intake Temperature (°C)";
                    }
                    else if (sh.X_axis_ID / 256 == 0xF9 && sh.Y_axis_ID / 256 == 0xE9)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Hydraulic fan speed normalization map (kuwHyLFTKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "kuwHyLFTKF";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "RPM";
                        //sh.X_axis_correction = 0.01;
                        //sh.Correction = 0.1;
                        sh.XaxisUnits = "RPM";
                        sh.X_axis_descr = "Fan speed (rpm)";
                        sh.Z_axis_descr = "Offset correction";
                    }
                    else if ((sh.X_axis_ID == 0xC19E && sh.Y_axis_ID == 0xE9E6 ) || (sh.X_axis_ID == 0xC16A && sh.Y_axis_ID == 0xE9D8) || (sh.X_axis_ID == 0xC1AE && sh.Y_axis_ID == 0xE9FC))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Radiator after-run pump time (kuwNLGRDKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "kuwNLGRDKF";
                        sh.Y_axis_correction = 0.015021;
                        //sh.X_axis_offset = -273.1;
                        sh.X_axis_correction = 0.1;
                        sh.X_axis_offset = -273.1;
                        sh.X_axis_descr = "Top of cylinder head Temperature (°C)";
                        sh.XaxisUnits = "°C";
                        //sh.X_axis_correction = 0.01;
                        sh.Correction = 0.02;
                        sh.YaxisUnits = "l/h";
                        sh.Y_axis_descr = "Water flow (l/h)";
                        sh.Z_axis_descr = "Run time (s)";
                        sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 16;
                        sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 36;
                    }
                    else if ((sh.X_axis_ID == 0xC19E && sh.Y_axis_ID == 0xC1B6) || (sh.X_axis_ID == 0xC1AE && sh.Y_axis_ID == 0xC1C6))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Radiator fan-caster correction map 2 (kuwNLKORK2) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "kuwNLKORK2";
                        sh.X_axis_correction = 0.1;
                        sh.X_axis_offset = -273.1;
                        sh.Y_axis_correction = 0.1;
                        sh.Y_axis_offset = -273.1;
                        sh.Y_axis_descr = "Top of cylinder head Temperature (°C)";
                        sh.YaxisUnits = "°C";
                        //sh.X_axis_correction = 0.01;
                        //sh.Correction = 0.1;
                        sh.XaxisUnits = "°C";
                        sh.X_axis_descr = "Ambient Intake Temperature (°C)";
                        sh.Z_axis_descr = "Correction (s)";
                    }
                    else if ((sh.X_axis_ID == 0xE9D4 && sh.Y_axis_ID == 0xF94A) || (sh.X_axis_ID == 0xE9C6 && sh.Y_axis_ID == 0xF948) || (sh.X_axis_ID == 0xE9EA && sh.Y_axis_ID == 0xF94A))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Electric cooling fan basic speed map (kuwElGRDKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "kuwElGRDKF";
                        sh.Y_axis_descr = "Cooling output (W)";
                        sh.YaxisUnits = "W";
                        //sh.X_axis_correction = 0.01;
                        //sh.Correction = 0.1;
                        sh.XaxisUnits = "RPM";
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.Z_axis_descr = "Fan speed (rpm)";
                    }
                    else if ((sh.X_axis_ID == 0xE9CE && sh.Y_axis_ID == 0xF94A ) || (sh.X_axis_ID == 0xE9C0 && sh.Y_axis_ID == 0xF948) || (sh.X_axis_ID == 0xE9E4 && sh.Y_axis_ID == 0xF94A))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Hydraulic cooling fan basic speed map (kuwHyGRDKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "kuwHyGRDKF";
                        sh.Y_axis_descr = "Cooling output (W)";
                        sh.YaxisUnits = "W";
                        //sh.X_axis_correction = 0.01;
                        //sh.Correction = 0.1;
                        sh.XaxisUnits = "RPM";
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.Z_axis_descr = "Fan speed (rpm)";
                    }
                    else if ((sh.X_axis_ID == 0xE9C2 && sh.Y_axis_ID == 0xE9C0 ) || (sh.X_axis_ID == 0xE9B2 && sh.Y_axis_ID == 0xE9B0) || (sh.X_axis_ID == 0xE9D8 && sh.Y_axis_ID == 0xE9D6))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Relative cooling requirement control map (kuwSTEU_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "kuwSTEU_KF";
                        sh.Y_axis_correction = 0.1;
                        sh.X_axis_correction = 0.01;
                        sh.X_axis_descr = "Vehicle speed (km/h)";
                        sh.XaxisUnits = "KMH";
                        //sh.X_axis_correction = 0.01;
                        //sh.Correction = 0.1;
                        sh.YaxisUnits = "°C";
                        sh.Y_axis_descr = "Temperature difference (cooling vs. requirement) (°C)";
                        sh.Z_axis_descr = "Desired cooling power (W)";
                    }
                    else if ((sh.X_axis_ID == 0xC1B6 && sh.Y_axis_ID == 0xC1CC) || (sh.X_axis_ID == 0xC1C6 && sh.Y_axis_ID == 0xC1DC))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Setpoint temperature difference will map 3 (kuwSOLL3KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "kuwSOLL3KF";
                        sh.X_axis_correction = 0.1;
                        sh.X_axis_offset = -273.1;
                        sh.Y_axis_correction = 0.1;
                        sh.Y_axis_offset = -273.1;
                        sh.Y_axis_descr = "Cylinder head water Temperature (°C)";
                        sh.YaxisUnits = "°C";
                        //sh.X_axis_correction = 0.01;
                        //sh.Correction = 0.1;
                        sh.XaxisUnits = "°C";
                        sh.X_axis_descr = "Ambient Temperature (°C)";
                        sh.Z_axis_descr = "Setpoint temperature difference (°C)";
                    }
                    else if ((sh.X_axis_ID == 0xC134 && sh.Y_axis_ID == 0xEA00) || (sh.X_axis_ID == 0xC13C && sh.Y_axis_ID == 0xEA16))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Relative dynamic cooling requirement will map 4 (kuwSOLL4KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "kuwSOLL4KF";
                        sh.X_axis_correction = 0.1;
                        //sh.X_axis_offset = -273.1;
                        sh.Y_axis_correction = 0.1;
                        //sh.Y_axis_offset = -273.1;
                        sh.Y_axis_descr = "Cylinder head outlet control Temperature (°C)";
                        sh.YaxisUnits = "°C";
                        //sh.X_axis_correction = 0.01;
                        //sh.Correction = 0.1;
                        sh.XaxisUnits = "°C";
                        sh.X_axis_descr = "Water Temperature difference (°C)";
                        sh.Z_axis_descr = "Correction (°C)";
                    }
                    else if ((sh.X_axis_ID == 0xC1CC && sh.Y_axis_ID == 0xE9B6) || (sh.X_axis_ID == 0xC1DC && sh.Y_axis_ID == 0xE9CC))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Load-dependend pre control cooling will map 2 (kuwSOLL2KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "kuwSOLL2KF";
                        sh.X_axis_correction = 0.1;
                        sh.X_axis_offset = -273.1;
                        sh.Y_axis_correction = 0.1;
                        sh.Y_axis_offset = -273.1;
                        sh.Y_axis_descr = "water Temperature setpoint (°C)";
                        sh.YaxisUnits = "°C";
                        //sh.X_axis_correction = 0.01;
                        sh.Correction = 0.1;
                        sh.Offset = -273.1;
                        sh.XaxisUnits = "°C";
                        sh.X_axis_descr = "Temperature (°C)";
                        sh.Z_axis_descr = "Load-dependent output Temperature (°C)";
                    }
                    else if ((sh.X_axis_ID == 0xF94A || sh.X_axis_ID == 0xF948) && sh.Y_axis_ID == 0xF944)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        int soll1Count = GetMapNameCountForCodeBlock("Water temperature basic setpoint -", sh.CodeBlock, newSymbols, false);
                        soll1Count--;
                        if (soll1Count % 2 == 1)
                        {
                            sh.Varname = "Water temperature basic setpoint - target will map 1 (kuwSOLL1KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "kuwSOLL1KF";
                            sh.Z_axis_descr = "Load-dependent output Temperature (°C)";
                        }
                        if (soll1Count % 2 == 0)
                        {
                            sh.Varname = "Water temperature basic setpoint - cooler basic map (kuwGRD_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "kuwGRD_KF";
                            sh.Z_axis_descr = "Temprature setpoint for cylinder head (°C)";
                        }
                        sh.YaxisUnits = "RPM";
                        sh.XaxisUnits = "mg/st";
                        sh.Y_axis_descr = "Engine speed (RPM)";
                        sh.X_axis_descr = "Injected Quantity (mg/stroke)";
                        sh.X_axis_correction = 0.01;
                        sh.Correction = 0.1;
                        sh.Offset = -273.1;
                    }
                    else if ((sh.X_axis_ID == 0xC19C && sh.Y_axis_ID == 0xEA24) || (sh.X_axis_ID == 0xC1AC && sh.Y_axis_ID == 0xEA3A))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Cooler temperature characteristic field (kmwLTKOR_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "kmwLTKOR_KF";
                        sh.Y_axis_correction = 0.1;
                        sh.Y_axis_offset = -273.1;
                        sh.Y_axis_descr = "Temperature (°C)";
                        sh.YaxisUnits = "°C";
                        //sh.X_axis_correction = 0.01;
                        sh.Correction = 0.1;
                        sh.Offset = -273.1;
                        sh.XaxisUnits = "mbar";
                        sh.X_axis_descr = "Pressure (mBar)";
                        sh.Z_axis_descr = "Output Temperature (°C)";
                    }
                    else if ((sh.X_axis_ID == 0xC270 && sh.Y_axis_ID == 0xC1B6) || (sh.X_axis_ID == 0xC234 && sh.Y_axis_ID == 0xC180) || (sh.X_axis_ID == 0xC27E && sh.Y_axis_ID == 0xC1C6))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Cooler setpoint correction map 2 (kmwKOR2_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "kmwKOR2_KF";
                        sh.X_axis_correction = 0.1;
                        sh.X_axis_offset = -273.1;
                        sh.X_axis_descr = "Ambient Temperature (°C)";
                        sh.XaxisUnits = "°C";
                        sh.Y_axis_correction = 0.01;
                        sh.Correction = 0.1;
                        sh.Offset = -273.1;
                        sh.YaxisUnits = "KMH";
                        sh.Y_axis_descr = "Vehicle speed (Km/h)";
                        sh.Z_axis_descr = "Output Temperature (°C)";
                    }
                    else if ((sh.X_axis_ID == 0xC134 && sh.Y_axis_ID == 0xC136) || (sh.X_axis_ID == 0xC13C && sh.Y_axis_ID == 0xC13E))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Water control duty cycle map (kmwSTEU_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "kmwSTEU_KF";
                        sh.X_axis_correction = 0.1;
                        sh.X_axis_offset = -273.1;
                        sh.X_axis_descr = "Setpoint Temperature (°C)";
                        sh.XaxisUnits = "°C";
                        sh.Y_axis_correction = 0.1;
                        sh.YaxisUnits = "°C";
                        sh.Y_axis_descr = "Control temperetare deviation (°C)";
                        sh.Z_axis_descr = "Duty Cycle %";
                    }
                    else if ((sh.X_axis_ID == 0xF94A && sh.Y_axis_ID == 0xC036) || (sh.X_axis_ID == 0xF948 && sh.Y_axis_ID == 0xC032) || (sh.X_axis_ID == 0xF94A && sh.Y_axis_ID == 0xC03A))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Boost correction basic setpoint map (ldwTWGRDKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "ldwTWGRDKF";
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.XaxisUnits = "RPM";
                        sh.Y_axis_correction = 0.01;
                        sh.YaxisUnits = "mgst";
                        sh.Y_axis_descr = "Injected Quantity (mg/stroke)";
                        sh.Z_axis_descr = "Boost correction (mbar)";
                        sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 16;
                        sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 36;
                    }
                    else if ((sh.X_axis_ID == 0xF94A && sh.Y_axis_ID == 0xC032) || (sh.X_axis_ID == 0xF948 && sh.Y_axis_ID == 0xC02E))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Airmass backup values map (arwLMVGWKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "arwLMVGWKF";
                        sh.X_axis_descr = "Boost pressure (mBar)";
                        sh.XaxisUnits = "MBAR";
                        sh.YaxisUnits = "RPM";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.Z_axis_descr = "Airmass (mg/stroke)";
                        sh.Correction = 0.1;
                    }
                    else if ((sh.X_axis_ID == 0xC0FC && sh.Y_axis_ID == 0xC1B0) || (sh.X_axis_ID == 0xC102 && sh.Y_axis_ID == 0xC1C0))
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Oil level treshold map (mrwOELNiKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "mrwOELNiKF";
                        sh.X_axis_descr = "Temperature (°C)";
                        sh.X_axis_correction = 0.1;
                        sh.X_axis_offset = -273.1;
                        sh.XaxisUnits = "°C";
                        sh.YaxisUnits = "RPM";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.Z_axis_descr = "Minimum Oil Level (mm)";
                        sh.Correction = 0.25;
                    }
                    /*else
                    {
                        sh.Category = "Debug detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                    }*/

                }
                else if (sh.Length == 150)  // 3L (1.2 TDi, three cylinder VW Lupo) has this
                {
                    if (sh.X_axis_length == 3 && sh.Y_axis_length == 25)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "Torque limiter [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Z_axis_descr = "Maximum IQ (mg)";
                        sh.Y_axis_descr = "Atm. pressure (mbar)";
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.Correction = 0.01;
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mbar";

                    }
                }
                else if (sh.Length == 138)  // R3 (1.4 TDi, three cylinder) has this
                {
                    if (sh.X_axis_length == 3 && sh.Y_axis_length == 23)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "Torque limiter [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Z_axis_descr = "Maximum IQ (mg)";
                        sh.Y_axis_descr = "Atm. pressure (mbar)";
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.Correction = 0.01;
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mbar";

                    }
                }
                else if (sh.Length == 132)  // R3 (1.4 TDi, three cylinder) has this
                {
                    if (sh.X_axis_length == 3 && sh.Y_axis_length == 22)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "Torque limiter [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Z_axis_descr = "Maximum IQ (mg)";
                        sh.Y_axis_descr = "Atm. pressure (mbar)";
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.Correction = 0.01;
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mbar";

                    }
                }
                else if (sh.Length == 126)
                {
                    if (sh.X_axis_length == 3 && sh.Y_axis_length == 21)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "Torque limiter [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Z_axis_descr = "Maximum IQ (mg)";
                        sh.Y_axis_descr = "Atm. pressure (mbar)";
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mbar";
                        sh.Correction = 0.01;
                    }
                }
                else if (sh.Length == 120)
                {
                    if (sh.X_axis_length == 3 && sh.Y_axis_length == 20)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Limiters";
                        sh.Varname = "Torque limiter [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Z_axis_descr = "Maximum IQ (mg)";
                        sh.Y_axis_descr = "Atm. pressure (mbar)";
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.Correction = 0.01;
                        sh.XaxisUnits = "rpm";
                        sh.YaxisUnits = "mbar";

                    }
                    else if (sh.X_axis_length == 6 && sh.Y_axis_length == 10)
                    {
                        if (sh.X_axis_ID / 256 == 0xF9 && sh.Y_axis_ID / 256 == 0xC5)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            int advStartCount = GetMapNameCountForCodeBlock("Injection advance at start map", sh.CodeBlock, newSymbols, false);
                            advStartCount--;
                            sh.Varname = "Injection advance at start map " + advStartCount.ToString("D2") + " [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "fnwSWSTxKR";
                            sh.X_axis_descr = "Temperature (°C)";
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.Z_axis_descr = "Crankshaft degrees (°)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 11328 - (advStartCount * 120);
                            sh.Correction = 0.0234375;
                            sh.Offset = -78;
                            sh.XaxisUnits = "°C";
                            sh.YaxisUnits = "rpm";
                        }
                        else if (sh.X_axis_ID / 256 == 0xC1 && sh.Y_axis_ID / 256 == 0xC1)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "CAN Desired engine speed map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "mrwNwunVE";
                            sh.X_axis_descr = "Temperature (°C)";
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.Z_axis_descr = "Speed (rpm)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.XaxisUnits = "°C";
                            sh.YaxisUnits = "rpm";
                        }
                    }
                    else if (sh.X_axis_length == 10 && sh.Y_axis_length == 6)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Water temperature dependent load IQ increase [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "mrwBWT_KF";
                        sh.X_axis_descr = "Temperature (°C)";
                        sh.X_axis_correction = 0.1;
                        sh.X_axis_offset = -273.1;
                        sh.Z_axis_descr = "IQ (mg/stroke)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.Correction = 0.01;
                        sh.XaxisUnits = "°C";
                        sh.YaxisUnits = "rpm";
                    }
                    /*else if (sh.X_axis_length == 10 && sh.Y_axis_length == 6)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "BIP [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        //sh.Z_axis_descr = "Maximum IQ (mg)";
                        //sh.Y_axis_descr = "Atm. pressure (mbar)";
                        //sh.X_axis_descr = "Engine speed (rpm)";
                        //sh.Correction = 0.01;
                        //sh.XaxisUnits = "rpm";
                        //sh.YaxisUnits = "mbar";

                    }*/
                }
                else if (sh.Length == 110)
                {
                    if (sh.X_axis_length == 11 && sh.Y_axis_length == 5)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Motor mount contol duty cycle (mlwTW_KF)[" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "rpm";
                        sh.X_axis_descr = "Injected quantity (mg/stroke)";
                        sh.XaxisUnits = "mgst";
                        sh.Z_axis_descr = "Duty cycle (%)";
                        sh.X_axis_correction = 0.01;
                    }
                }
                else if (sh.Length == 100)
                {
                    if (sh.X_axis_length == 10 && sh.Y_axis_length == 5)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Acceleration limitation map (mrwBdn_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.YaxisUnits = "rpm";
                        sh.X_axis_descr = "Water temperature (°C)";
                        sh.XaxisUnits = "°C";
                        sh.Y_axis_correction = 0.2384;
                        sh.X_axis_offset = -273.1;
                        sh.X_axis_correction = 0.1;
                        sh.Z_axis_descr = "IQ Limit (mg/stroke)";
                        sh.Correction = 0.01;
                    }
                }
                else if (sh.Length == 98)
                {
                    if (sh.X_axis_length == 7 && sh.Y_axis_length == 7)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Generator load torque loss map (mrwDFMD_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.XaxisUnits = "rpm";
                        sh.Y_axis_descr = "Generator load (%)";
                        sh.YaxisUnits = "%";
                        sh.Y_axis_correction = 0.01;
                        //sh.Y_axis_correction = 0.2384;
                        sh.Z_axis_descr = "Torque loss (nm)";
                        sh.Correction = 0.1;
                        sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 14;
                        sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 32;
                    }
                }
                else if (sh.Length == 80)
                {
                    if (sh.X_axis_length == 8 && sh.Y_axis_length == 5)
                    {
                        if (sh.X_axis_ID / 256 == 0xF9 && sh.Y_axis_ID / 256 == 0xE9)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            int soll1Count = GetMapNameCountForCodeBlock("ARD Ramp rate load change -", sh.CodeBlock, newSymbols, false);
                            soll1Count--;
                            if (soll1Count % 2 == 1)
                            {
                                sh.Varname = "ARD Ramp rate load change - positive quantity trend[" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwFPRA_KF";
                                sh.Z_axis_descr = "mg/stroke per second";
                            }
                            if (soll1Count % 2 == 0)
                            {
                                sh.Varname = "ARD Ramp rate load change - negative quantity trend[" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwFNRA_KF";
                                sh.Z_axis_descr = "mg/stroke per second";
                            }
                            sh.YaxisUnits = "RPM";
                            sh.XaxisUnits = "gear";
                            sh.Y_axis_descr = "Engine speed (RPM)";
                            sh.X_axis_descr = "Selected gear";
                            sh.Correction = 0.15;
                        }
                        else if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xC1)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Parallel control engine temperature correction map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "arwSTTWKF";
                            sh.X_axis_descr = "Temperature (°C)";
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.Z_axis_descr = "Correction (%)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.XaxisUnits = "°C";
                            sh.YaxisUnits = "rpm";
                            sh.Correction = 0.01;
                        }
                    }
                    else if (sh.X_axis_length == 10 && sh.Y_axis_length == 4)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Parallel control engine altitude correction map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "arwSTPAKF";
                        sh.X_axis_descr = "Ambient air pressure (mBar)";
                        sh.Z_axis_descr = "Correction (%)";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.XaxisUnits = "mbar";
                        sh.YaxisUnits = "rpm";
                        sh.Correction = 0.01;
                    }
                }
                else if (sh.Length == 72)
                {
                    if (sh.X_axis_length == 6 && sh.Y_axis_length == 6)
                    {
                        if (sh.X_axis_ID / 256 == 0xF9 && sh.Y_axis_ID / 256 == 0xF9)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Glow plugs glow control map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "gswTV4_KF";
                            sh.Y_axis_descr = "Injected quantity (mg/stroke)";
                            sh.Z_axis_descr = "Glow plug cycle (%)";
                            sh.X_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "mgst";
                            sh.XaxisUnits = "rpm";
                            sh.Correction = 0.01;
                            sh.Y_axis_correction = 0.01;
                        }
                        else if (sh.X_axis_ID / 256 == 0xC1 && sh.Y_axis_ID / 256 == 0xC0)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Cooling fan demand map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "kuwANF_KF";
                            sh.X_axis_descr = "Intake Air Temperature (°C)";
                            sh.Z_axis_descr = "Output demand (W)";
                            sh.Y_axis_descr = "Air Pressure (mbar)";
                            sh.XaxisUnits = "°C";
                            sh.YaxisUnits = "mbar";
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 12;
                            sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 28;
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }
                }
                else if (sh.Length == 70)
                {
                    if (sh.X_axis_length == 5 && sh.Y_axis_length == 7)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Engine Oil soot level map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Userdescription = "siwOEL_rKF";
                        sh.X_axis_descr = "Injected quantity (mg/stroke)";
                        sh.Z_axis_descr = "Engine oil soot load index";
                        sh.Y_axis_descr = "Engine speed (rpm)";
                        sh.XaxisUnits = "mgst";
                        sh.YaxisUnits = "rpm";
                        sh.X_axis_correction = 0.01;
                    }
                }
                else if (sh.Length == 64)
                {
                    if (sh.X_axis_length == 32 && sh.Y_axis_length == 1)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "MAF linearization [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                    }
                }

                else if (sh.Length == 60)
                {
                    if (sh.Y_axis_length == 6 && sh.X_axis_length == 5)
                    {
                        if (sh.Y_axis_ID == 0xC1A2 || sh.Y_axis_ID == 0xC16C)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "EGR temperature map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.X_axis_descr = "Temperature"; //IAT, ECT or Fuel temp?
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.Z_axis_descr = "Mass airflow correction";
                        }
                    }
                }
                else if (sh.Length == 50)
                {
                    if (sh.X_axis_length == 25 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID == 0xEBB8 || sh.X_axis_ID == 0xEBBA)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Ambient temperature sensor linearization map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_descr = "mV";
                            sh.YaxisUnits = "mv";
                            sh.Z_axis_descr = "Temperature (°C)";
                            sh.Correction = 0.1;
                            sh.Offset = -273.1;
                            sh.Y_axis_correction = 4.888;
                        }
                        else if (sh.X_axis_ID == 0xEBB4 || sh.X_axis_ID == 0xEBB6)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Water temperature sensor linearization map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_descr = "mV";
                            sh.YaxisUnits = "mv";
                            sh.Z_axis_descr = "Temperature (°C)";
                            sh.Correction = 0.1;
                            sh.Offset = -273.1;
                            sh.Y_axis_correction = 4.888;
                        }
                        else if (sh.X_axis_ID / 256 == 0xEC)
                        {
                            int sortCount = GetMapNameCountForCodeBlock("  ", sh.CodeBlock, newSymbols, false);
                            sortCount--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "  [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address);
                            if (sortCount % 2 == 1)
                            {
                                sh.Varname = "Advanced thrust characteristic map  [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwSCHUPKL";
                                sh.Z_axis_descr = "Temperature corrected taget quantity (mg/stroke)";
                                sh.Category = "Detected maps";
                                sh.Subcategory = "Misc";
                                sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 50;
                                sh.Correction = 0.01;
                                sh.Y_axis_descr = "Engine speed (rpm)";
                                sh.YaxisUnits = "rpm";
                            }
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }
                    else if (sh.X_axis_length == 5 && sh.Y_axis_length == 5)
                    {
                        if (sh.X_axis_ID / 256 == 0xC1 && sh.Y_axis_ID / 256 == 0xC0)
                        {
                            sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 10;
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 24;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Water critical temperature basic map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "kuwWTFkrKF";
                            sh.X_axis_descr = "Water temperature at cylinder head (°C)";
                            sh.Z_axis_descr = "Critical temperature (°C)";
                            sh.Y_axis_descr = "Filtered consumption (l/h)";
                            sh.XaxisUnits = "°C";
                            sh.YaxisUnits = "l/h";
                            sh.X_axis_correction = 0.1;
                            //sh.X_axis_offset = -273.1;
                            sh.Correction = 0.1;
                            sh.Offset = -273.1;
                            sh.Y_axis_correction = 0.15;
                        }
                        else if (sh.X_axis_ID / 256 == 0xC1 && sh.Y_axis_ID / 256 == 0xC1)
                        {
                            sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 10;
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 24;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            int kuwNlCount = GetMapNameCountForCodeBlock("Cooler run-on characteristic map -", sh.CodeBlock, newSymbols, false);
                            kuwNlCount--;
                            if (kuwNlCount % 2 == 1)
                            {
                                sh.Varname = "Cooler run-on characteristic map - Electic operated fan [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "kuwNlEL_KF";
                                sh.Z_axis_descr = "Cooler output (%)";
                            }
                            if (kuwNlCount % 2 == 0)
                            {
                                sh.Varname = "Cooler run-on characteristic map - Water operated fan [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "kunNlHy_KF";
                                sh.Z_axis_descr = "Cooler output (%)";
                            }
                            sh.Y_axis_descr = "Water temperature at cylinder head (°C)";
                            sh.X_axis_descr = "Ambient temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.XaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.Correction = 0.01;
                        }
                        else if (sh.X_axis_ID / 256 == 0xF9 && sh.Y_axis_ID / 256 == 0xE9)
                        {
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 10;
                            sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 24;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            int kuwNlCount = GetMapNameCountForCodeBlock("Load impact detection speed/gear map -", sh.CodeBlock, newSymbols, false);
                            kuwNlCount--;
                            if (kuwNlCount % 2 == 1)
                            {
                                sh.Varname = "Load impact detection speed/gear map - Positive [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwDLS_pos";
                                sh.Z_axis_descr = "Injected Quantity (mg/stroke)";
                            }
                            if (kuwNlCount % 2 == 0)
                            {
                                sh.Varname = "Load impact detection speed/gear map - Negative [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwDLS_neg";
                                sh.Z_axis_descr = "Injected Quantity (mg/stroke)";
                            }
                            sh.Y_axis_descr = "Selected gear";
                            sh.X_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "gear";
                            sh.XaxisUnits = "rpm";
                            sh.Correction = 0.01;
                        }
                        else if ((sh.X_axis_ID == 0xC1AA || sh.X_axis_ID == 0xC174) && sh.Y_axis_ID / 256 == 0xEC)
                        {
                            //sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 10;
                            //sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 24;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Altitude dependent speed correction map (mrwBATM_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "mrwBATM_KF";
                            sh.X_axis_descr = "Engine speed (rpm)";
                            sh.Z_axis_descr = "Correction";
                            sh.Y_axis_descr = "Atmospheric pressure (mBar)";
                            sh.XaxisUnits = "rpm";
                            sh.YaxisUnits = "mbar";
                            //sh.X_axis_correction = 0.1;
                            //sh.X_axis_offset = -273.1;
                            sh.Correction = 0.0001;
                        }
                        else if ((sh.X_axis_ID == 0xC1B0 || sh.X_axis_ID == 0xC17A) && sh.Y_axis_ID / 256 == 0xEC)
                        {
                            //sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 10;
                            //sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 24;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Oil temperature overheat protection map (mrwBOEL_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "mrwBOEL_KF";
                            sh.X_axis_descr = "Engine speed (rpm)";
                            sh.Z_axis_descr = "Correction";
                            sh.Y_axis_descr = "Oil temperature (°C)";
                            sh.XaxisUnits = "rpm";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Correction = 0.0001;
                        }
                        else if (sh.X_axis_ID / 256 == 0xC2 && sh.Y_axis_ID / 256 == 0xF9)
                        {
                            //sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 10;
                            //sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 24;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "CVT Transmission drag torque limitation map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "mrwSchmxKF";
                            sh.X_axis_descr = "Engine speed (rpm)";
                            sh.Z_axis_descr = "Correction";
                            sh.Y_axis_descr = "Speed (km/h)";
                            sh.XaxisUnits = "rpm";
                            sh.YaxisUnits = "kmh";
                            sh.Y_axis_correction = 0.01;
                            //sh.Y_axis_offset = -273.1;
                            sh.Correction = 0.0001;
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }
                    /*else
                    {
                        sh.Category = "Debug detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                    }*/
                }

                else if (sh.Length == 48)
                {
                    if (sh.X_axis_length == 6 && sh.Y_axis_length == 4)
                    {
                        sh.Category = "Detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Water temperature compensation map (anwWTFkoKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        sh.Y_axis_correction = 0.1;
                        sh.Correction = 0.1;
                        sh.XaxisUnits = "rpm";
                        sh.X_axis_descr = "Engine speed (rpm)";
                        sh.Z_axis_descr = "Compensation (°C)";
                        sh.Y_axis_descr = "Temperature (°C)";
                        sh.YaxisUnits = "°C";
                    }
                }
                else if (sh.Length == 40)
                {
                    if (sh.Y_axis_length == 4 && sh.X_axis_length == 5)
                    {
                        if (sh.X_axis_ID / 256 == 0xC1 && sh.Y_axis_ID / 256 == 0xC0)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            //sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.X_axis_descr = "Air pressure (mbar)";
                            sh.Y_axis_descr = "Temperature (°C)";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.YaxisUnits = "°C";
                            sh.XaxisUnits = "mbar";
                            sh.Z_axis_descr = "Target speed (rpm)";
                            sh.Varname = "Target idle speed map (mrwWTAD_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID / 256 == 0xC1 && sh.Y_axis_ID / 256 == 0xC1)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            //sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.X_axis_descr = "Temperature (°C)";
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.XaxisUnits = "°C";
                            sh.YaxisUnits = "rpm";
                            sh.Z_axis_descr = "Correction";
                            sh.Correction = 0.0001;
                            sh.Varname = "Weighting torque correction map (mrwKFTkorr) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }
                    }
                    else if (sh.Y_axis_length == 5 && sh.X_axis_length == 4)
                    {
                        if (sh.X_axis_ID / 256 == 0xC1 && sh.Y_axis_ID / 256 == 0xC0)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            //sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.X_axis_descr = "Air pressure (mbar)";
                            sh.Y_axis_descr = "Temperature (°C)";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.YaxisUnits = "°C";
                            sh.XaxisUnits = "mbar";
                            sh.Z_axis_descr = "Correction factor";
                            sh.Correction = 0.0001;
                            sh.Varname = "Water temp full load increase correction map (mrwBPL_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                    }
                    else if (sh.X_axis_length == 20 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID / 256 == 0xF9)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                            sh.Correction = 0.01;
                            sh.Varname = "EGR Shutdown timed treshold map (arwMEAB2KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID / 256 == 0xC1)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Water temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Z_axis_descr = "Permissible time (s)";
                            sh.Y_axis_offset = -273.1;
                            sh.Y_axis_correction = 0.1;
                            sh.Correction = 0.02;
                            sh.Varname = "Permissible heating time map (mrwWTF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID / 256 == 0xEC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            int arwREGCount = GetMapNameCountForCodeBlock("EGR shutdown treshold -", sh.CodeBlock, newSymbols, false);
                            arwREGCount--;
                            if (arwREGCount % 2 == 0)
                            {
                                sh.Varname = "EGR shutdown treshold - lower hysteresis [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwMEAB0KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (arwREGCount % 2 == 1)
                            {
                                sh.Varname = "EGR shutdown treshold - upper hysteresis [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwMEAB1KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }
                    /*else
                    {
                        sh.Category = "Debug detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                    }*/
                }
                else if (sh.Length == 36)
                {
                    if (sh.X_axis_length == 18 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID == 0xEBB2 || sh.X_axis_ID == 0xEBB4)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "mV";
                            sh.YaxisUnits = "mV";
                            sh.Z_axis_descr = "Temperature (°C)";
                            sh.Offset = -273.1;
                            sh.Correction = 0.1;
                            sh.Y_axis_correction = 4.88758;
                            sh.Varname = "Intake manifold temperature sensor linearization (anwSTF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEB96)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "mV";
                            sh.YaxisUnits = "mV";
                            sh.Z_axis_descr = "Temperature (°C)";
                            sh.Offset = -273.1;
                            sh.Correction = 0.1;
                            sh.Y_axis_correction = 4.88758;
                            sh.Varname = "Cylinder head water temperature sensor linearization (anwWTF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            eb96CylHeadPresent = true;
                        }
                        else if (sh.X_axis_ID == 0xEB90)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "mV";
                            sh.YaxisUnits = "mV";
                            sh.Z_axis_descr = "Temperature (°C)";
                            sh.Offset = -273.1;
                            sh.Correction = 0.1;
                            sh.Y_axis_correction = 4.88758;
                            sh.Varname = "Fuel temperature sensor linearization (anwKTF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEB92)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "mV";
                            sh.YaxisUnits = "mV";
                            sh.Z_axis_descr = "Temperature (°C)";
                            sh.Offset = -273.1;
                            sh.Correction = 0.1;
                            sh.Y_axis_correction = 4.88758;
                            if (eb96CylHeadPresent == true)
                            {
                                sh.Varname = "Fuel temperature sensor linearization (anwKTF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            }
                            else
                            {
                                sh.Varname = "Air temperature sensor linearization (anwLTF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            }
                        }
                        else if (sh.X_axis_ID == 0xEB94)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "mV";
                            sh.YaxisUnits = "mV";
                            sh.Z_axis_descr = "Temperature (°C)";
                            sh.Offset = -273.1;
                            sh.Correction = 0.1;
                            sh.Y_axis_correction = 4.88758;
                            if (eb96CylHeadPresent == true)
                            {
                                sh.Varname = "Air temperature sensor linearization (anwLTF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            }
                            else
                            {
                                sh.Varname = "Cylinder head water temperature sensor linearization (anwWTF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            }
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }
                    else if (sh.X_axis_length == 6 && sh.Y_axis_length == 3)
                    {
                        if (sh.X_axis_ID / 256 == 0xC0 && sh.Y_axis_ID / 256 == 0xEA)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            //sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.Y_axis_descr = "Air pressure (mbar)";
                            sh.X_axis_descr = "Injected quantity (mg/stroke)";
                            sh.X_axis_correction = 0.01;
                            sh.XaxisUnits = "mgst";
                            sh.YaxisUnits = "mbar";
                            sh.Z_axis_descr = "Correction factor (%)";
                            sh.Correction = 0.0001;
                            sh.Varname = "Boost pressure correction characteristic map (ldwTVPAKF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                    }
                    /*else
                    {
                        sh.Category = "Debug detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                    }*/
                }
                else if (sh.Length == 32)
                {
                    if (sh.X_axis_length == 16 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID / 256 == 0xC0)
                        {
                            int pidCount = GetMapNameCountForCodeBlock("PID map -", sh.CodeBlock, newSymbols, false);
                            pidCount--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Turbo";
                            sh.Y_axis_correction = 0.015022;
                            sh.YaxisUnits = "l/h";
                            sh.Y_axis_descr = "Consumption (l/h)";
                            sh.Z_axis_descr = "Output PID value multiplication";
                            sh.Correction = 0.0001;
                            if (pidCount % 4 == 3)
                            {
                                sh.Varname = "PID map - P amplification (ldwPRfakKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            }
                            if (pidCount % 4 == 2)
                            {
                                sh.Varname = "PID map - DT1 memory factor (ldwDR_gfKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            }
                            if (pidCount % 4 == 1)
                            {
                                sh.Varname = "PID map - D amplification (ldwDRfakKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            }
                            if (pidCount % 4 == 0)
                            {
                                sh.Varname = "PID map - I amplification (ldwIRfakKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            }
                        }
                        else if (sh.X_axis_ID / 256 == 0xEC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.YaxisUnits = "rpm";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.Z_axis_descr = "Actuator limit (%)";
                            sh.Correction = 0.01;
                            sh.Varname = "Boost actuator upper limit curve (ldwDRmaxKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                    }
                    else if (sh.X_axis_length == 4 && sh.Y_axis_length == 4)
                    {
                        if (sh.X_axis_ID / 256 == 0xC1 && sh.Y_axis_ID / 256 == 0xC1)
                        {
                            int klwTCount = GetMapNameCountForCodeBlock("Air compressor switch-off time map -", sh.CodeBlock, newSymbols, false);
                            klwTCount--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.XaxisUnits = "°C";
                            sh.X_axis_descr = "Engine temperature (°C)";
                            sh.Y_axis_descr = "Air Pressure (mbar)";
                            sh.YaxisUnits = "mbar";
                            sh.Z_axis_descr = "Switch-off time (s)";
                            sh.Correction = 0.01;
                            sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 8;
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 20;
                            if (klwTCount % 2 == 0)
                            {
                                sh.Varname = "Air compressor switch-off time map - min (klwTMIN_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            }
                            if (klwTCount % 2 == 1)
                            {
                                sh.Varname = "Air compressor switch-off time map - max (klwTMAX_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            }
                        }
                        else if (sh.X_axis_ID / 256 == 0xC0 && sh.Y_axis_ID / 256 == 0xDB)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Smoke/boost pressure compensation map (mrwKFPkorr) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_correction = 0.0001;
                            sh.YaxisUnits = "-";
                            sh.Y_axis_descr = "Multiplicative factors";
                            sh.X_axis_descr = "Boost Pressure (mbar)";
                            sh.XaxisUnits = "mbar";
                            sh.Z_axis_descr = "Correction";
                            sh.Correction = 0.0001;
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 8;
                            sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 20;
                        }
                        else if (sh.X_axis_ID / 256 == 0xF9 && sh.Y_axis_ID / 256 == 0xC1)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Air conditioning torque requirement map (mrwKLMD_KF) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.XaxisUnits = "rpm";
                            sh.X_axis_descr = "Engine speed (rpm)";
                            sh.Y_axis_descr = "Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Z_axis_descr = "Torque loss due to air conditioning (NM)";
                            sh.Correction = 0.1;
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 8;
                            sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 20;
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }
                }
                else if (sh.Length == 30)
                {
                    if (sh.X_axis_length == 15 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID / 256 == 0xEA)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Water heat dissipation map (kmw_ThHzKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_descr = "Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Z_axis_descr = "Temperature (°C)";
                            sh.Correction = 0.0000244141;
                        }
                    }
                    else
                    {
                        sh.Category = "Debug detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                    }
                }
                else if (sh.Length == 26)
                {
                    if (sh.X_axis_length == 13 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID / 256 == 0xE9)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "EGR Air linearization map (arwLMBLIKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_descr = "MAF sensor raw value (%)";
                            sh.YaxisUnits = "%";
                            sh.Y_axis_correction = 0.01;
                            sh.Z_axis_descr = "Air flow (kg/h)";
                            sh.Correction = 0.1;
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }

                }
                else if (sh.Length == 24)
                {
                    if (sh.X_axis_length == 12 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID / 256 == 0xEA)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Water heat map based on combustion (kmw_ThMeKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_descr = "Flow (l/h)";
                            sh.YaxisUnits = "l/h";
                            sh.Y_axis_correction = 0.01502200489;
                            sh.Z_axis_descr = "Temperature (°C)";
                            sh.Correction = 0.0000244141;
                        }
                        else if (sh.X_axis_ID / 256 == 0xEC || sh.X_axis_ID / 256 == 0xF9)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "ASG-ECO Mode torque limiter (mrwBDB2_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                            sh.Correction = 0.01;
                        }
                    }
                    else
                    {
                        sh.Category = "Debug detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                    }
                }
                else if (sh.Length == 22)
                {
                    if (sh.X_axis_length == 11 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID / 256 == 0xEC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Starting torque quantity characterization (mrwANFAHKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.Z_axis_descr = "Air flow (mg/stroke)";
                            sh.Correction = 0.01;
                        }
                    }
                    /*else
                    {
                        sh.Category = "Debug detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                    }*/
                }
                else if (sh.Length == 20)
                {
                    if (sh.Y_axis_length == 5 && sh.X_axis_length == 2)
                    {
                        //if (sh.Y_axis_ID == 0xC1A2)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            //sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.Y_axis_descr = "Air pressure";
                            sh.X_axis_descr = "Temperature"; //IAT, ECT or Fuel temp?
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.Z_axis_descr = "Time (sec)";
                            sh.Correction = 0.01;
                            sh.Varname = "Pre-glow map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                    }
                    if (sh.X_axis_length == 10 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID == 0xEBB6)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "mV";
                            sh.YaxisUnits = "mv";
                            sh.Y_axis_correction = 4.887585532747;
                            sh.Z_axis_descr = "Temperature (°C)";
                            sh.Correction = 0.1;
                            sh.Offset = -273.1;
                            sh.Varname = "Heating linearization map (anwHZA_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC5E2 || sh.X_axis_ID == 0xC57E || sh.X_axis_ID == 0xC5FE)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Z_axis_descr = "Selected Map";
                            sh.Correction = 0.00390625;
                            sh.Varname = "Selector for start of injection map (fnwSWGK_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC19E)
                        {
                            int ldwRCount = GetMapNameCountForCodeBlock("Boost control -", sh.CodeBlock, newSymbols, false);
                            ldwRCount--;
                            if (ldwRCount % 2 == 1)
                            {
                                sh.Varname = "Boost control - maximum positive deviation (ldwRMXpRKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwRMXpRKL";
                                sh.Z_axis_descr = "Maximum deviation (mbar)";
                                //sh.Correction = 0.01;
                            }
                            if (ldwRCount % 2 == 0)
                            {
                                sh.Varname = "Boost control - cold start shutdown water characteristic map (ldwKSTWKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwKSTWKL";
                                sh.Z_axis_descr = "Boost switch-off time (s)";
                                sh.Correction = 0.01;
                            }
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                        }
                        else if (sh.X_axis_ID == 0xC16A)
                        {
                            int ldwRCount = GetMapNameCountForCodeBlock("Boost control -", sh.CodeBlock, newSymbols, false);
                            ldwRCount--;
                            if (ldwRCount % 3 == 1)
                            {
                                sh.Varname = "Boost control - maximum positive deviation (ldwRMXpRKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwRMXpRKL";
                                sh.Z_axis_descr = "Maximum deviation (mbar)";
                                //sh.Correction = 0.01;
                            }
                            if (ldwRCount % 3 == 0)
                            {
                                sh.Varname = "Boost control - cold start shutdown water characteristic map (ldwKSTWKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwKSTWKL";
                                sh.Z_axis_descr = "Boost switch-off time (s)";
                                sh.Correction = 0.01;
                            }
                            if (ldwRCount % 3 == 2)
                            {
                                sh.Varname = "Boost control -(fakemap probably)";
                                //sh.Userdescription = "ldwKSTWKL";
                                //sh.Z_axis_descr = "Boost switch-off time (s)";
                                //sh.Correction = 0.01;
                            }
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                        }
                        else if (sh.X_axis_ID == 0xC19C || sh.X_axis_ID == 0xC168 || sh.X_axis_ID == 0xC1AC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Ambient temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Z_axis_descr = "Time to engine-off (s)";
                            sh.Correction = 0.1;
                            sh.Varname = "Engine-off command delay time characteristic map (khwUTF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEA3C || sh.X_axis_ID == 0xEA52 || sh.X_axis_ID == 0xE9FE)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Ambient temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Z_axis_descr = "Temperature treshold (°C)";
                            sh.Correction = 0.1;
                            sh.Offset = -273.1;
                            sh.Varname = "Cooling water heating characteristic map (khwKHTL_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEC2E)
                        {
                            int ece2Count = GetMapNameCountForCodeBlock("   ", sh.CodeBlock, newSymbols, false);
                            ece2Count--;
                            sh.Y_axis_descr = "Engine speed (rpm";
                            sh.YaxisUnits = "rpm";
                            //sh.Y_axis_correction = 0.1;
                            //sh.Y_axis_offset = -273.1;
                            if (ece2Count % 7 == 6)
                            {
                                sh.Varname = "   Boost pressure control ON treshold line (ldwREG1KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwREG1KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (ece2Count % 7 == 5)
                            {
                                sh.Varname = "   Boost pressure control OFF treshold line (ldwREG0WKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwREG0WKL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (ece2Count % 7 == 4)
                            {
                                sh.Varname = "   (fakemap)";
                                //sh.Userdescription = "ldwREG0WKL";
                                //sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                //sh.Correction = 0.01;
                            }
                            if (ece2Count % 7 == 3)
                            {
                                sh.Varname = "   (fakemap) ";
                                //sh.Userdescription = "ldwREG0WKL";
                                //sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                //sh.Correction = 0.01;
                            }
                            if (ece2Count % 7 == 2)
                            {
                                sh.Varname = "   EGR control small-quantity on characteristic line (arwREG1KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwREG1KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                                sh.Y_axis_descr = "Engine speed (rpm)";
                                sh.YaxisUnits = "rpm";
                                sh.Y_axis_correction = 1;
                                sh.Y_axis_offset = 0;
                            }
                            if (ece2Count % 7 == 1)
                            {
                                sh.Varname = "   EGR control small-quantity off characteristic line (arwREG0KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwREG0KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                                sh.Y_axis_descr = "Engine speed (rpm)";
                                sh.YaxisUnits = "rpm";
                                sh.Y_axis_correction = 1;
                                sh.Y_axis_offset = 0;
                            }
                            if (ece2Count % 7 == 0)
                            {
                                sh.Varname = "   Boost intake manifold vacuum detection curve (mrwLDFU_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwLDFU_KL";
                                sh.Z_axis_descr = "Pressure difference (mbar)";
                                //sh.Correction = 0.01;
                            }
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                        }
                        else if (sh.X_axis_ID == 0xEC38)
                        {
                            int ece2Count = GetMapNameCountForCodeBlock("   ", sh.CodeBlock, newSymbols, false);
                            ece2Count--;
                            sh.Y_axis_descr = "Engine speed (rpm";
                            sh.YaxisUnits = "rpm";
                            //sh.Y_axis_correction = 0.1;
                            //sh.Y_axis_offset = -273.1;
                            if (ece2Count % 7 == 6)
                            {
                                sh.Varname = "   Boost pressure control ON treshold line (ldwREG1KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwREG1KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (ece2Count % 7 == 5)
                            {
                                sh.Varname = "   Boost pressure control OFF treshold line (ldwREG0WKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwREG0WKL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (ece2Count % 7 == 4)
                            {
                                sh.Varname = "   (fakemap)";
                                //sh.Userdescription = "ldwREG0WKL";
                                //sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                //sh.Correction = 0.01;
                            }
                            if (ece2Count % 7 == 3)
                            {
                                sh.Varname = "   (fakemap) ";
                                //sh.Userdescription = "ldwREG0WKL";
                                //sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                //sh.Correction = 0.01;
                            }
                            if (ece2Count % 7 == 2)
                            {
                                sh.Varname = "   EGR control small-quantity on characteristic line (arwREG1KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwREG1KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                                sh.Y_axis_descr = "Engine speed (rpm)";
                                sh.YaxisUnits = "rpm";
                                sh.Y_axis_correction = 1;
                                sh.Y_axis_offset = 0;
                            }
                            if (ece2Count % 7 == 1)
                            {
                                sh.Varname = "   EGR control small-quantity off characteristic line (arwREG0KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwREG0KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                                sh.Y_axis_descr = "Engine speed (rpm)";
                                sh.YaxisUnits = "rpm";
                                sh.Y_axis_correction = 1;
                                sh.Y_axis_offset = 0;
                            }
                            if (ece2Count % 7 == 0)
                            {
                                sh.Varname = "   Boost intake manifold vacuum detection curve (mrwLDFU_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwLDFU_KL";
                                sh.Z_axis_descr = "Pressure difference (mbar)";
                                //sh.Correction = 0.01;
                            }
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                        }
                        else if (sh.X_axis_ID == 0xEC2C)
                        {
                            int ece2Count = GetMapNameCountForCodeBlock("   ", sh.CodeBlock, newSymbols, false);
                            ece2Count--;
                            sh.Y_axis_descr = "Engine speed (rpm";
                            sh.YaxisUnits = "rpm";
                            //sh.Y_axis_correction = 0.1;
                            //sh.Y_axis_offset = -273.1;
                            if (ece2Count % 7 == 5)
                            {
                                sh.Varname = "   Boost pressure control ON treshold line (ldwREG1KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwREG1KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (ece2Count % 7 == 4)
                            {
                                sh.Varname = "   Boost pressure control OFF treshold line (ldwREG0WKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwREG0WKL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (ece2Count % 7 == 3)
                            {
                                sh.Varname = "   (fakemap)";
                                //sh.Userdescription = "ldwREG0WKL";
                                //sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                //sh.Correction = 0.01;
                            }
                            if (ece2Count % 7 == 2)
                            {
                                sh.Varname = "   (fakemap) ";
                                //sh.Userdescription = "ldwREG0WKL";
                                //sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                //sh.Correction = 0.01;
                            }
                            if (ece2Count % 7 == 1)
                            {
                                sh.Varname = "   EGR control small-quantity on characteristic line (arwREG1KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwREG1KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                                sh.Y_axis_descr = "Engine speed (rpm)";
                                sh.YaxisUnits = "rpm";
                                sh.Y_axis_correction = 1;
                                sh.Y_axis_offset = 0;
                            }
                            if (ece2Count % 7 == 0)
                            {
                                sh.Varname = "   EGR control small-quantity off characteristic line (arwREG0KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwREG0KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                                sh.Y_axis_descr = "Engine speed (rpm)";
                                sh.YaxisUnits = "rpm";
                                sh.Y_axis_correction = 1;
                                sh.Y_axis_offset = 0;
                            }
                            if (ece2Count % 7 == 6)
                            {
                                sh.Varname = "   Boost intake manifold vacuum detection curve (mrwLDFU_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwLDFU_KL";
                                sh.Z_axis_descr = "Pressure difference (mbar)";
                                //sh.Correction = 0.01;
                            }
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                        }
                        else if (sh.X_axis_ID == 0xC1A2 || sh.X_axis_ID == 0xC16C || sh.X_axis_ID == 0xC1B2)
                        {
                            int c1a2Count = GetMapNameCountForCodeBlock("Delay - ", sh.CodeBlock, newSymbols, false);
                            c1a2Count--;
                            sh.Y_axis_descr = "Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            if (c1a2Count % 2 == 1)
                            {
                                sh.Varname = "Delay - EGR start time after startup (anwANSTWKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "anwANSTWKL";
                                sh.Z_axis_descr = "Time (s)";
                                sh.Correction = 0.01;
                            }
                            if (c1a2Count % 2 == 0)
                            {
                                sh.Varname = "Delay - Starting IQ after startup (mrwSTMFRKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwSTMFRKL";
                                sh.Z_axis_descr = "Time (s)";
                                sh.Correction = 0.01;
                            }
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                        }
                        else if (sh.X_axis_ID == 0xF94A)
                        {
                            int f94aCount = GetMapNameCountForCodeBlock("Map -", sh.CodeBlock, newSymbols, false);
                            f94aCount--;
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            //sh.Y_axis_correction = 0.1;
                            //sh.Y_axis_offset = -273.1;
                            if (f94aCount % 5 == 4)
                            {
                                sh.Varname = "Map - time for delayed shutdown (arwTi_abKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwTi_abKL";
                                sh.Z_axis_descr = "Time (s)";
                                sh.Correction = 0.01;
                            }
                            if (f94aCount % 5 == 3)
                            {
                                sh.Varname = "Map - Minimum IQ control upper treshold (zmwMEmi1KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "zmwMEmi1KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (f94aCount % 5 == 2)
                            {
                                sh.Varname = "Map - Minimum IQ control lower treshold (zmwMEmi0KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "zmwMEmi0KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (f94aCount % 5 == 1)
                            {
                                sh.Varname = "Map - Boost intake manifold vacuum state cancellation curve (mrwLDFO_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwLDFO_KL";
                                sh.Z_axis_descr = "Pressure difference (mbar)";
                                //sh.Correction = 0.01;
                            }
                            if (f94aCount % 5 == 0)
                            {
                                sh.Varname = "Map - Boundary acceleration limited quantity curve (mrwBdnN_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwBdnN_KL";
                                sh.Z_axis_descr = "Correction factor";
                                sh.Correction = 0.0001;
                            }
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                        }
                        else if (sh.X_axis_ID == 0xF948)
                        {
                            int f94aCount = GetMapNameCountForCodeBlock("Map -", sh.CodeBlock, newSymbols, false);
                            f94aCount--;
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            //sh.Y_axis_correction = 0.1;
                            //sh.Y_axis_offset = -273.1;
                            if (f94aCount % 4 == 2)
                            {
                                sh.Varname = "Map - Minimum IQ control upper treshold (zmwMEmi1KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "zmwMEmi1KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (f94aCount % 4 == 1)
                            {
                                sh.Varname = "Map - Minimum IQ control lower treshold (zmwMEmi0KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "zmwMEmi0KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (f94aCount % 4 == 3)
                            {
                                sh.Varname = "Map - Boost intake manifold vacuum state cancellation curve (mrwLDFO_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwLDFO_KL";
                                sh.Z_axis_descr = "Pressure difference (mbar)";
                                //sh.Correction = 0.01;
                            }
                            if (f94aCount % 4 == 0)
                            {
                                sh.Varname = "Map - Boundary acceleration limited quantity curve (mrwBdnN_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwBdnN_KL";
                                sh.Z_axis_descr = "Correction factor";
                                sh.Correction = 0.0001;
                            }
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                        }
                        else if (sh.X_axis_ID == 0xC1AE || sh.X_axis_ID == 0xC178 || sh.X_axis_ID == 0xC1BE)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Fuel Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Z_axis_descr = "Correction factor";
                            sh.Correction = 0.00390625;
                            sh.Varname = "BIP - Fuel temperature correction curve (zmwBPKorKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEC5E || sh.X_axis_ID == 0xEC5C || sh.X_axis_ID == 0xEC68)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Fuel Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            //sh.Y_axis_offset = -273.1;
                            sh.Z_axis_descr = "Corrected fuel Temperature (°C)";
                            sh.Correction = 0.1;
                            sh.Varname = "Fuel temperature correction curve (zmwMKorKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC5BA || sh.X_axis_ID == 0xC556 || sh.X_axis_ID == 0xC5D6)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Voltage (mV)";
                            sh.YaxisUnits = "mv";
                            sh.Y_axis_correction = 20.372434017595;
                            //sh.Y_axis_offset = -273.1;
                            sh.Z_axis_descr = "Time (microseconds)";
                            //sh.Correction = 0.1;
                            sh.Varname = "BIP - Basic voltage linearization map (zmwBPGndKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEB98 || sh.X_axis_ID == 0xEB9A)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Voltage (mV)";
                            sh.YaxisUnits = "mv";
                            sh.Y_axis_correction = 4.887585532747;
                            //sh.Y_axis_offset = -273.1;
                            sh.Z_axis_descr = "Sensor (%)";
                            sh.Correction = 0.01;
                            sh.Varname = "RME Sensor linearization map (anwRME_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }

                }

                else if (sh.Length >= 18 && sh.Length <= 70)
                {
                    if (sh.X_axis_ID / 16 == 0xC1A && sh.Y_axis_ID / 16 == 0xEC3)
                    {

                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Limiters";
                            //Temp after intercooler
                            sh.Y_axis_descr = "Temperature";
                            sh.X_axis_descr = "Engine speed (rpm)"; //IAT, ECT or Fuel temp?
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Z_axis_descr = "%";
                            sh.Correction = 0.01;
                            sh.Varname = "IQ by air intake temp[" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                    }
                    else if (sh.X_axis_length == 3 && sh.Y_axis_length == 3)
                    {
                        if (sh.X_axis_ID / 256 == 0xC1 && sh.Y_axis_ID / 256 == 0xEC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Air Temperature (°C)";
                            sh.X_axis_descr = "Engine speed (rpm)";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Z_axis_descr = "Correction factor";
                            sh.Correction = 0.0001;
                            sh.XaxisUnits = "rpm";
                            sh.YaxisUnits = "°C";
                            sh.Varname = "Charged air overheat protection map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID / 256 == 0xC1 && sh.Y_axis_ID / 256 == 0xC2)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Water Temperature (°C)";
                            sh.X_axis_descr = "Vehicle speed (km/h)";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.X_axis_correction = 0.01;
                            sh.Z_axis_descr = "Correction factor";
                            sh.Correction = 0.0001;
                            sh.XaxisUnits = "km/h";
                            sh.YaxisUnits = "°C";
                            sh.Varname = "Water overheat protection map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                    }
                    /*else
                    {
                        sh.Category = "Debug detected maps";
                        sh.Subcategory = "Misc";
                        sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                    }*/

                    /*else if (sh.X_axis_length == 16 && sh.Y_axis_length == 1 && sh.X_axis_ID / 256 == 0xC0)
                    {

                                sh.Category = "Detected maps";
                                sh.Subcategory = "Turbo";
                                sh.Varname = "PID [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                    }*/

                }
                else if (sh.Length == 16)
                {
                    if (sh.X_axis_length == 8 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID == 0xC1B0 || sh.X_axis_ID == 0xC17A || sh.X_axis_ID == 0xC1C0)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Oil temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Z_axis_descr = "Correction (s)";
                            sh.Correction = 0.02;
                            sh.Varname = "Oil temperature correction line for overrun (kuwNLOELKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID / 256 == 0xF9)
                        {
                            int f9Count = GetMapNameCountForCodeBlock("Map-", sh.CodeBlock, newSymbols, false);
                            f9Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            if (f9Count % 8 == 7)
                            {
                                sh.Varname = "Map- Cooler water temp speed correction line (kuwKOR1_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "kuwKOR1_KL";
                                sh.Z_axis_descr = "Correction factor";
                                sh.Correction = 0.0001;
                            }
                            if (f9Count % 8 == 6)
                            {
                                sh.Varname = "Map- EGR cooler bypass switch-off line (arwERGnAus) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwERGnAus";
                                sh.Z_axis_descr = "Air (mg/stroke)";
                                sh.Correction = 0.1;
                            }
                            if (f9Count % 8 == 5)
                            {
                                sh.Varname = "Map- EGR cooler bypass switch-on line (arwERGnEin) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwERGnEin";
                                sh.Z_axis_descr = "Air (mg/stroke)";
                                sh.Correction = 0.1;
                            }
                            if (f9Count % 8 == 4)
                            {
                                sh.Varname = "Map- Boost shutdown freezing time characteristic (ldwVZDZ_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwVZDZ_KL";
                                sh.Z_axis_descr = "Factor";
                                sh.Correction = 0.0001;
                            }
                            if (f9Count % 8 == 3)
                            {
                                sh.Varname = "Map- ARD shaper gradient bandwidth positive trend - negative offset (mrwFPoU_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwFPoU_KL";
                                sh.Z_axis_descr = "Injected Quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (f9Count % 8 == 2)
                            {
                                sh.Varname = "Map- ARD shaper gradient bandwidth positive trend - positive offset (mrwFPoO_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwFPoO_KL";
                                sh.Z_axis_descr = "Injected Quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (f9Count % 8 == 1)
                            {
                                sh.Varname = "Map- ARD shaper gradient bandwidth negative trend - negative offset (mrwFNoU_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwFNoU_KL";
                                sh.Z_axis_descr = "Injected Quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (f9Count % 8 == 0)
                            {
                                sh.Varname = "Map- ARD shaper gradient bandwidth negative trend - positive offset (mrwFNoO_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwFNoO_KL";
                                sh.Z_axis_descr = "Injected Quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                        }
                        else if (sh.X_axis_ID == 0xE9D8 || sh.X_axis_ID == 0xE9CA || sh.X_axis_ID == 0xE9EE) 
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.Z_axis_descr = "Duty cycle (%)";
                            sh.Correction = 0.01;
                            sh.Varname = "Electric fan speed characterization curve (kuwElLFTKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEA22 || sh.X_axis_ID == 0xEA38)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Water temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_offset = -273.1;
                            sh.Y_axis_correction = 0.1;
                            sh.Z_axis_descr = "Corrected temperature (°C)";
                            sh.Correction = 0.1;
                            sh.Offset = -273.1;
                            sh.Varname = "Ambient temperature water correction line (kmwWTkorKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xE972 || sh.X_axis_ID == 0xE968 || sh.X_axis_ID == 0xE97E)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "%";
                            sh.YaxisUnits = "%";
                            sh.Y_axis_correction = 0.01;
                            sh.Z_axis_descr = "Freeze time (s)";
                            sh.Correction = 0.01;
                            sh.Varname = "Boost shutdown freeze time curve (ldwVZAR_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC1B8 || sh.X_axis_ID == 0xC182 || sh.X_axis_ID == 0xC1C8)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Pedal position (%)";
                            sh.YaxisUnits = "%";
                            sh.Y_axis_correction = 0.01;
                            sh.Z_axis_descr = "Engine speed (rpm)";
                            sh.Varname = "Pedal position linearization map (mrwADR_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC1A2 || sh.X_axis_ID == 0xC1B2)
                        {
                            int c1a2Count = GetMapNameCountForCodeBlock("EGR-", sh.CodeBlock, newSymbols, false);
                            c1a2Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            if (c1a2Count % 2 == 0)
                            {
                                sh.Varname = "EGR- Engine temperature dependend drop map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwUMDRpKL";
                                sh.Z_axis_descr = "Rotations since start";
                            }
                            if (c1a2Count % 2 == 1)
                            {

                            }
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }
                }
                else if (sh.Length == 14)
                {
                    if (sh.X_axis_length == 7 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID == 0xC5E0 || sh.X_axis_ID == 0xC57C || sh.X_axis_ID == 0xC5FC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Camshaft position (°)";
                            sh.YaxisUnits = "°";
                            sh.Y_axis_correction = 0.0234375;
                            sh.Z_axis_descr = "Correction (°)";
                            sh.Correction = 0.0234375;
                            sh.Varname = "Twisted camshaft torsion correction angle map (zmwNWkoKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC19E || sh.X_axis_ID == 0xC1AE)
                        {
                            int c19eCount = GetMapNameCountForCodeBlock("Water temp dependent - ", sh.CodeBlock, newSymbols, false);
                            c19eCount--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Water Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            if (c19eCount % 2 == 1)
                            {
                                sh.Varname = "Water temp dependent - Maximum deviation factor b/w internal/external transmission (mrwFVHGDKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwFVHGDKL";
                                sh.Z_axis_descr = "Deviation";
                                sh.Correction = 0.0001;
                            }
                            if (c19eCount % 2 == 0)
                            {
                                sh.Varname = "Water temp dependent -  LLR-Integrator initial value characteristic line (mrwSTINILL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwSTINILL";
                                sh.Z_axis_descr = "Value";
                                sh.Correction = 0.01;
                            }
                        }
                        else if (sh.X_axis_ID == 0xC16A)
                        {
                            int c19eCount = GetMapNameCountForCodeBlock("Water temp dependent - ", sh.CodeBlock, newSymbols, false);
                            c19eCount--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Water Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            if (c19eCount % 2 == 0)
                            {
                                sh.Varname = "Water temp dependent - Maximum deviation factor b/w internal/external transmission (mrwFVHGDKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwFVHGDKL";
                                sh.Z_axis_descr = "Deviation";
                                sh.Correction = 0.0001;
                            }
                            if (c19eCount % 2 == 1)
                            {
                                sh.Varname = "Water temp dependent -  LLR-Integrator initial value characteristic line (mrwSTINILL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwSTINILL";
                                sh.Z_axis_descr = "Value";
                                sh.Correction = 0.01;
                            }
                        }
                        else if (sh.X_axis_ID == 0xC19C || sh.X_axis_ID == 0xC168)
                        {
                            int c19cCount = GetMapNameCountForCodeBlock("IAT dependent - ", sh.CodeBlock, newSymbols, false);
                            c19cCount--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Air Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            if (c19cCount % 3 == 2)
                            {
                                sh.Varname = "IAT dependent - Boost correction (ldwTVTLKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwTVTLKL";
                                sh.Z_axis_descr = "Correction";
                                sh.Correction = 0.0001;
                            }
                            if (c19cCount % 3 == 1)
                            {
                                sh.Varname = "IAT dependent - Injection correction (ldwTLUEKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwTLUEKL";
                                sh.Z_axis_descr = "Correction";
                                sh.Correction = 0.0001;
                            }
                            if (c19cCount % 3 == 0)
                            {
                                sh.Varname = "IAT dependent - Smoke/Boost correction (mrwTSTLKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwTSTLKL";
                                sh.Z_axis_descr = "Value";
                                sh.Correction = 0.0001;
                            }
                        }
                        else if (sh.X_axis_ID == 0xEA9E)
                        {
                            int ea9eCount = GetMapNameCountForCodeBlock("IAT dependent - ", sh.CodeBlock, newSymbols, false);
                            ea9eCount--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Air Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            if (ea9eCount % 2 == 1)
                            {
                                sh.Varname = "IAT dependent - Boost correction (ldwTVTLKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwTVTLKL";
                                sh.Z_axis_descr = "Correction";
                                sh.Correction = 0.0001;
                            }
                            if (ea9eCount % 2 == 0)
                            {
                                sh.Varname = "IAT dependent - Injection correction (ldwTLUEKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwTLUEKL";
                                sh.Z_axis_descr = "Correction";
                                sh.Correction = 0.0001;
                            }
                        }
                        else if (sh.X_axis_ID == 0xC1AC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Air Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Varname = "IAT dependent- Smoke/Boost correction (mrwTSTLKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "mrwTSTLKL";
                            sh.Z_axis_descr = "Value";
                            sh.Correction = 0.0001;
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }
                }
                else if (sh.Length == 12)
                {
                    if (sh.X_axis_length == 6 && sh.Y_axis_length == 1)
                    {
                        if ((sh.X_axis_ID & 0xFFF0) == 0xECB0)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Fuel";
                            sh.Varname = "Selector for injector duration [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            // soi values as axis!!
                            sh.Y_axis_correction = -0.023437;
                            sh.Y_axis_offset = 78;
                            sh.Correction = 0.00390625;
                            sh.Z_axis_descr = "Map index";

                            sh.YaxisUnits = "SOI";
                        }
                        else if (sh.X_axis_ID == 0xEB8E || sh.X_axis_ID == 0xEB90)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Value";
                            sh.YaxisUnits = "-";
                            sh.Z_axis_descr = "Temperature (°C)";
                            sh.Correction = 0.1;
                            sh.Offset = -273.1;
                            sh.Varname = "Digital/analog Temperature correction map (anwUTF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC030 || sh.X_axis_ID == 0xC034)
                        {
                            int c030Count = GetMapNameCountForCodeBlock("Altitude dependent - ", sh.CodeBlock, newSymbols, false);
                            c030Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Altitude pressure (mbar)";
                            sh.YaxisUnits = "mbar";
                            if (c030Count % 4 == 3)
                            {
                                sh.Varname = "Altitude dependent - SOI Pressure correction map selector (fnwSWAD_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "fnwSWAD_KL";
                                sh.Z_axis_descr = "fnwSWADxKR map";
                                sh.Correction = 0.00390625;
                            }
                            if (c030Count % 4 == 2)
                            {
                                sh.Varname = "Altitude dependent - Smoke turbo boost limit negative ramp (mrwTSADnKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwTSADnKL";
                                sh.Z_axis_descr = "Limit (mg/stroke/s)";
                                sh.Correction = 0.5;
                            }
                            if (c030Count % 4 == 1)
                            {
                                sh.Varname = "Altitude dependent - Smoke turbo boost limit positive ramp (mrwTSADpKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwTSADpKL";
                                sh.Z_axis_descr = "Limit (mg/stroke/s)";
                                sh.Correction = 0.5;
                            }
                            if (c030Count % 4 == 0)
                            {
                                sh.Varname = "Altitude dependent - EGR setpoint characteristic map (arwPSKORKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwPSKORKL";
                                sh.Z_axis_descr = "Air quantity (mg/stroke)";
                                sh.Correction = 0.1;
                            }
                        }
                        else if (sh.X_axis_ID == 0xC02C)
                        {
                            int c030Count = GetMapNameCountForCodeBlock("Altitude dependent - ", sh.CodeBlock, newSymbols, false);
                            c030Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Altitude pressure (mbar)";
                            sh.YaxisUnits = "mbar";
                            if (c030Count % 4 == 3)
                            {
                                sh.Varname = "Altitude dependent - SOI Pressure correction map selector (fnwSWAD_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "fnwSWAD_KL";
                                sh.Z_axis_descr = "fnwSWADxKR map";
                                sh.Correction = 0.00390625;
                            }
                            if (c030Count % 4 == 2)
                            {
                                sh.Varname = "Altitude dependent - Smoke turbo boost limit negative ramp (mrwTSADnKL) ? [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwTSADnKL";
                                sh.Z_axis_descr = "Limit (mg/stroke/s)";
                                sh.Correction = 0.5;
                            }
                            if (c030Count % 4 == 1)
                            {
                                sh.Varname = "Altitude dependent - Smoke turbo boost limit positive ramp (mrwTSADpKL) ? [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwTSADpKL";
                                sh.Z_axis_descr = "Limit (mg/stroke/s)";
                                sh.Correction = 0.5;
                            }
                            if (c030Count % 4 == 0)
                            {
                                sh.Varname = "Altitude dependent - EGR setpoint characteristic map (arwPSKORKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwPSKORKL";
                                sh.Z_axis_descr = "Air quantity (mg/stroke)";
                                sh.Correction = 0.1;
                            }
                        }
                        else if (sh.X_axis_ID == 0xEC2E || sh.X_axis_ID == 0xEC2C)
                        {
                            int ece2Count = GetMapNameCountForCodeBlock("Speed dependent - ", sh.CodeBlock, newSymbols, false);
                            ece2Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            if (ece2Count % 6 == 5)
                            {
                                sh.Varname = "Speed dependent - Heating generator off treshold line (khwKH_ABKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "khwKH_ABKL";
                                sh.Z_axis_descr = "Generator treshold (%)";
                                sh.Correction = 0.01;
                            }
                            if (ece2Count % 6 == 4)
                            {
                                sh.Varname = "Speed dependent - Heating generator on treshold line (khwKH_ZUKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "khwKH_ZUKL";
                                sh.Z_axis_descr = "Generator treshold (%)";
                                sh.Correction = 0.01;
                            }
                            if (ece2Count % 6 == 3)
                            {
                                sh.Varname = "Speed dependent - Boost actuator lower limit line (ldwGRminKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwGRminKL";
                                sh.Z_axis_descr = "Actuator treshold (%)";
                                sh.Correction = 0.01;
                            }
                            if (ece2Count % 6 == 2)
                            {
                                sh.Varname = "Speed dependent - Ratiometric processing memory factor HFM5 line (anwGFH51KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "anwGFH51KL";
                                sh.Z_axis_descr = "-";
                                sh.Correction = 0.000030517578;
                            }
                            if (ece2Count % 6 == 1)
                            {
                                sh.Varname = "Speed dependent - IQ limit in case of error (mrwBEM_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwBEM_KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (ece2Count % 6 == 0)
                            {
                                sh.Varname = "Speed dependent - Quantity limit treshold with active VE (mrwBMVE_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwBMVE_KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                        }
                        else if (sh.X_axis_ID == 0xEC38)
                        {
                            int ece2Count = GetMapNameCountForCodeBlock("Speed dependent - ", sh.CodeBlock, newSymbols, false);
                            ece2Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            if (ece2Count % 4 == 3)
                            {
                                sh.Varname = "Speed dependent - Heating generator off treshold line (khwKH_ABKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "khwKH_ABKL";
                                sh.Z_axis_descr = "Generator treshold (%)";
                                sh.Correction = 0.01;
                            }
                            if (ece2Count % 4 == 2)
                            {
                                sh.Varname = "Speed dependent - Boost actuator lower limit line (ldwGRminKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "ldwGRminKL";
                                sh.Z_axis_descr = "Actuator treshold (%)";
                                sh.Correction = 0.01;
                            }
                            if (ece2Count % 4 == 1)
                            {
                                sh.Varname = "Speed dependent - Ratiometric processing memory factor HFM5 line (anwGFH51KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "anwGFH51KL";
                                sh.Z_axis_descr = "-";
                                sh.Correction = 0.000030517578;
                            }
                            if (ece2Count % 4 == 0)
                            {
                                sh.Varname = "Speed dependent - IQ limit in case of error (mrwBEM_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwBEM_KL";
                                sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                        }
                        else if (sh.X_axis_ID == 0xC270 || sh.X_axis_ID == 0xC27E)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Speed (km/h)";
                            sh.YaxisUnits = "km/h";
                            sh.Z_axis_descr = "Corrected speed (km/h)";
                            sh.Correction = 0.01;
                            sh.Y_axis_correction = 0.01;
                            sh.Varname = "Cooling fan speed correction curve (kuwANKORKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC260 || sh.X_axis_ID == 0xC26E)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Capacity (%)";
                            sh.YaxisUnits = "%";
                            sh.Z_axis_descr = "Corrected capacity (%)";
                            sh.Correction = 0.01;
                            sh.Y_axis_correction = 0.01;
                            sh.Varname = "CAN relative cooling capacity characteristic curve (kuwTV_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xE9F2 || sh.X_axis_ID == 0xEA08)
                        {
                            int e9f2Count = GetMapNameCountForCodeBlock("Cooling fan - ", sh.CodeBlock, newSymbols, false);
                            e9f2Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Vehicle speed (km/h)";
                            sh.YaxisUnits = "km/h";
                            sh.Y_axis_correction = 0.01;
                            if (e9f2Count % 2 == 1)
                            {
                                sh.Varname = "Cooling fan - Hydraulic minimum speed [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "kuwHyminKL";
                                sh.Z_axis_descr = "Minimum speed (rpm)";
                            }
                            if (e9f2Count % 2 == 0)
                            {
                                sh.Varname = "Cooling fan - Electric minimum speed [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "kuwElminKL";
                                sh.Z_axis_descr = "Minimum speed (rpm)";
                            }
                        }
                        else if (sh.X_axis_ID == 0xE9E2 || sh.X_axis_ID == 0xE9D4 || sh.X_axis_ID == 0xE9F8)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "refrigerant pressure (Bar)";
                            sh.YaxisUnits = "Bar";
                            sh.Z_axis_descr = "Cooling capacity (W)";
                            sh.Y_axis_correction = 0.002;
                            sh.Varname = "Cooler refrigerant pressure characteristic line (kuwKVM_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC1A2 || sh.X_axis_ID == 0xC16C || sh.X_axis_ID == 0xC1B2)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Z_axis_descr = "Engine speed (rpm)";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Varname = "Engine temperature-dependent idle start speed curve (mrwLLW_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC5BA || sh.X_axis_ID == 0xC556 || sh.X_axis_ID == 0xC5D6)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Battery Voltage (V)";
                            sh.YaxisUnits = "V";
                            sh.Z_axis_descr = "Maximum Amps driveable by Injector (A)";
                            sh.Correction = 0.033294232649;
                            sh.Y_axis_correction = 0.020372434017595;
                            sh.Varname = "BIP Start-up max Battery voltage dependent line (zmwBPAnIKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEBE8 || sh.X_axis_ID == 0xEBEA)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Time (uS)";
                            sh.YaxisUnits = "uS";
                            sh.Z_axis_descr = "Imp/m";
                            sh.Varname = "Kienzle tachograph route factor characteristic curve (fgwSF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xDABC || sh.X_axis_ID == 0xDAC0)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Powertrain factor";
                            sh.Y_axis_correction = 0.01;
                            sh.Correction = 0.000030517578;
                            sh.YaxisUnits = "-";
                            sh.Z_axis_descr = "-";
                            sh.Varname = "Conversion filter characteristic map (mrwFVHFIKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC19E)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Water temperature (°C)";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Correction = 0.01;
                            sh.YaxisUnits = "°C";
                            sh.Z_axis_descr = "CVT Engine speed factor (%)";
                            sh.Varname = "CVT Water temperature characteristic curve (mrwCWTFKOR) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }
                }
                else if (sh.Length == 10)
                {
                    if (sh.X_axis_length == 5 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID == 0xC19C || sh.X_axis_ID == 0xC168 || sh.X_axis_ID == 0xC1AC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Air Temperature (°C)";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Correction = 0.1;
                            sh.YaxisUnits = "°C";
                            sh.Z_axis_descr = "Oil temperature substitute added value (°C)";
                            sh.Varname = "Oil temperature subst. value curve (anwO_LUrKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC0F4 || sh.X_axis_ID == 0xC0E6 || sh.X_axis_ID == 0xC0FA )
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Water flow rate (l/h)";
                            sh.Y_axis_correction = 0.01502200489;
                            sh.Correction = 0.1;
                            sh.YaxisUnits = "l/h";
                            sh.Z_axis_descr = "Oil temperature substitute added value (°C)";
                            sh.Varname = "Oil temperature subst. value curve (anwO_VBtKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC5E2 || sh.X_axis_ID == 0xC57E || sh.X_axis_ID == 0xC5FE)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Water Temperature (°C)";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Correction = 0.00390625;
                            sh.YaxisUnits = "°C";
                            sh.Z_axis_descr = "Dynamic advance map selected";
                            sh.Varname = "Injector dynamic advance selector map (fnwSWDY_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC1A2 || sh.X_axis_ID == 0xC16C || sh.X_axis_ID == 0xC1B2)
                        {
                            int c1a2Count = GetMapNameCountForCodeBlock("Engine temp dependent - ", sh.CodeBlock, newSymbols, false);
                            c1a2Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Engine temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            if (c1a2Count % 3 == 2)
                            {
                                sh.Varname = "Engine temp dependent - Idle speed maximum revolutions after start count curve (mrwWTUMDKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwWTUMDKL";
                                sh.Z_axis_descr = "Revolutions";
                            }
                            if (c1a2Count % 3 == 1)
                            {
                                sh.Varname = "Engine temp dependent - Starter motor disengagement engine speed characteristic map (mrwSTNB_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwSTNB_KL";
                                sh.Z_axis_descr = "Engine speed (rpm)";
                            }
                            if (c1a2Count % 3 == 0)
                            {
                                sh.Varname = "Engine temp dependent - Starter motor after-time disengagement engine speed characteristic map (mrwSTNO_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwSTNO_KL";
                                sh.Z_axis_descr = "Engine speed (rpm)";
                            }
                        }
                        else if (sh.X_axis_ID == 0xC1A6 || sh.X_axis_ID == 0xC170 || sh.X_axis_ID == 0xC1B6)
                        {
                            int c1a6Count = GetMapNameCountForCodeBlock("Coolant setpoint - ", sh.CodeBlock, newSymbols, false);
                            c1a6Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Heating requirement (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            if (c1a6Count % 2 == 1)
                            {
                                sh.Varname = "Coolant setpoint - No climatronic correction characteristic map (kmwKOR4_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "kmwKOR4_KL";
                                sh.Z_axis_descr = "Set Temperature (°C)";
                                sh.Correction = 0.1;
                                sh.Offset = -273.1;
                            }
                            if (c1a6Count % 2 == 0)
                            {
                                sh.Varname = "Coolant setpoint - Heating requirement maximum selection curve (kmwKOR5_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "kmwKOR5_KL";
                                sh.Z_axis_descr = "Set temperature (°C)";
                                sh.Correction = 0.1;
                                sh.Offset = -273.1;
                            }
                        }
                        else if (sh.X_axis_ID == 0xC1B0 || sh.X_axis_ID == 0xC17A || sh.X_axis_ID == 0xC1C0)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Oil temperature (°C)";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Correction = 0.1;
                            sh.Offset = -273.1;
                            sh.YaxisUnits = "°C";
                            sh.Z_axis_descr = "Temperature correction (°C)";
                            sh.Varname = "Coolant setpoint Oil temperature correction curve (kmwKOR3_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEC2E || sh.X_axis_ID == 0xEC2C)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.Correction = 0.01;
                            sh.YaxisUnits = "rpm";
                            sh.Z_axis_descr = "Injected quantity offset (mg/stroke)";
                            sh.Varname = "ARD limitation offset for limiting amount (mrwABegOKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEC38)
                        {
                            int ec38Count = GetMapNameCountForCodeBlock("ARD limitation", sh.CodeBlock, newSymbols, false);
                            ec38Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.YaxisUnits = "rpm";
                            if (ec38Count % 2 == 0)
                            {
                                sh.Varname = "ARD limitation offset for limiting amount (mrwABegOKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwABegOKL"; 
                                sh.Z_axis_descr = "Injected quantity offset (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (ec38Count % 2 == 1)
                            {
                                sh.Varname = "ARD limitation unknown map (probably fake or not even ARD related) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "";
                            }
                        }
                        else if (sh.X_axis_ID == 0xE940 || sh.X_axis_ID == 0xE936 || sh.X_axis_ID == 0xE94C)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Air quantity (mg/stroke Air)";
                            sh.Correction = 0.01;
                            sh.Y_axis_correction = 0.1;
                            sh.YaxisUnits = "mgst";
                            sh.Z_axis_descr = "Pulse duty factor (%)";
                            sh.Varname = "Parallel control air quantity correction curve (arwMLTVKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC19E || sh.X_axis_ID == 0xC16A || sh.X_axis_ID == 0xC1AE)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Water temperature (°C)";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.YaxisUnits = "°C";
                            sh.Z_axis_descr = "Engine speed (rpm)";
                            sh.Varname = "EGR overrun shutdown upper treshold curve (arwREGSBN) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC0FC || sh.X_axis_ID == 0xC0EE || sh.X_axis_ID == 0xC102)
                        {
                            int c0fcCount = GetMapNameCountForCodeBlock("ARD Disturbance controller - ", sh.CodeBlock, newSymbols, false);
                            c0fcCount--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "ARD Filtered speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            if (c0fcCount % 4 == 3)
                            {
                                sh.Varname = "ARD Disturbance controller - Load impact detected upper limit (mrwARDDoKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwARDDoKL";
                                sh.Z_axis_descr = "Amount of intervention (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (c0fcCount % 4 == 2)
                            {
                                sh.Varname = "ARD Disturbance controller - Load impact detected lower limit (mrwARDDuKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwARDDuKL";
                                sh.Z_axis_descr = "Amount of intervention (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (c0fcCount % 4 == 1)
                            {
                                sh.Varname = "ARD Disturbance controller - Standard operation upper limit (mrwARDSoKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwARDSoKL";
                                sh.Z_axis_descr = "Amount of intervention (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (c0fcCount % 4 == 0)
                            {
                                sh.Varname = "ARD Disturbance controller - Standard operation lower limit (mrwARDSuKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwARDSuKL";
                                sh.Z_axis_descr = "Amount of intervention (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                        }
                        else if (sh.X_axis_ID == 0xC042 || sh.X_axis_ID == 0xC048)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Injected quantity (mg/stroke)";
                            sh.Y_axis_correction = 0.01;
                            sh.YaxisUnits = "mgst";
                            sh.Z_axis_descr = "Multiplicative correction factor";
                            sh.Correction = 0.0001;
                            sh.Varname = "EGR Motor temperature correction characteristic curve (arwMEKORKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }
                }
                else if (sh.Length == 8)
                {
                    if (sh.X_axis_length == 4 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID == 0xC030 || sh.X_axis_ID == 0xC02C || sh.X_axis_ID == 0xC034)
                        {
                            int c030Count = GetMapNameCountForCodeBlock("Pressure - ", sh.CodeBlock, newSymbols, false);
                            c030Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Air pressure (mBar)";
                            sh.YaxisUnits = "mbar";
                             if (c030Count % 2 == 1)
                            {
                                sh.Varname = "Pressure - Injection advance at start map selector (fnwSWST_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "fnwSWST_KL";
                                sh.Z_axis_descr = "Selected map";
                                sh.Correction = 0.00390625;
                            }
                            if (c030Count % 2 == 0)
                            {
                                sh.Varname = "Pressure - EGR Setpoint height correction (arwPAKORKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwPAKORKL";
                                sh.Z_axis_descr = "mg/stroke air";
                                sh.Correction = 0.1;
                            }
                        }
                        else if (sh.X_axis_ID == 0xC19C || sh.X_axis_ID == 0xC168 || sh.X_axis_ID == 0xC1AC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Air temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Z_axis_descr = "Selected map";
                            sh.Y_axis_offset = -273.1;
                            sh.Y_axis_correction = 0.1;
                            sh.Correction = 0.00390625;
                            sh.Varname = "Injection correction map (Air temperature) selector (fnwSWLT_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC1A2 || sh.X_axis_ID == 0xC16C || sh.X_axis_ID == 0xC1B2)
                        {
                            int c1a2Count = GetMapNameCountForCodeBlock("Glowplug control - ", sh.CodeBlock, newSymbols, false);
                            c1a2Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Engine temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            if (c1a2Count % 3 == 2)
                            {
                                sh.Varname = "Glowplug control - Indicator glow period (gswGAZ_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "gswGAZ_KL";
                                sh.Z_axis_descr = "Period (s)";
                                sh.Correction = 0.01;
                            }
                            if (c1a2Count % 3 == 1)
                            {
                                sh.Varname = "Glowplug control - glow period time in area 1 (gswGS_t1KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "gswGS_t1KL";
                                sh.Z_axis_descr = "Period (s)";
                                sh.Correction = 0.01;
                            }
                            if (c1a2Count % 3 == 0)
                            {
                                sh.Varname = "Glowplug control - Afterglow period (gswGS_NGKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "gswGS_NGKL";
                                sh.Z_axis_descr = "Period (s)";
                                sh.Correction = 0.01;
                            }
                        }
                        else if (sh.X_axis_ID == 0xC1B6 || sh.X_axis_ID == 0xC1C6)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Ambient temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Z_axis_descr = "Multiplication factor";
                            sh.Y_axis_offset = -273.1;
                            sh.Y_axis_correction = 0.1;
                            sh.Correction = 0.0001;
                            sh.Varname = "Coolant run-on time ambient temperature correction line (kuwNLF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC082 || sh.X_axis_ID == 0xC07E || sh.X_axis_ID == 0xC088)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Injected quantity (mg/stroke)";
                            sh.YaxisUnits = "mgst";
                            sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                            sh.Y_axis_correction = 0.01;
                            sh.Correction = 0.01;
                            sh.Varname = "Smooth-running controler LRR limiting amount characteristic (mrwLRR_BGR) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEB9A || sh.X_axis_ID == 0xEB9C)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "mV";
                            sh.YaxisUnits = "mV";
                            sh.Z_axis_descr = "%";
                            sh.Y_axis_correction = 4.887585532747;
                            sh.Correction = 0.01;
                            sh.Varname = "PGS linearization (not active) (anwPGS_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xF94A || sh.X_axis_ID == 0xF948)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.Z_axis_descr = "Injected quantity (mg/stroke)";
                            sh.Correction = 0.01;
                            sh.Varname = "ARD drivers wish limiter line (mrwFFBgrKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }
                    else if (sh.X_axis_length == 2 && sh.Y_axis_length == 2)
                    {
                        if (sh.X_axis_ID / 256 == 0xE9 && sh.Y_axis_ID / 256 == 0xEC)
                        {
                            int e9Count = GetMapNameCountForCodeBlock("EGR aroREG_1 - ", sh.CodeBlock, newSymbols, false);
                            e9Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.X_axis_descr = "Pulse duty factor (%)";
                            sh.XaxisUnits = "%";
                            sh.X_axis_correction = 0.01;
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 4;
                            sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 12;
                            if (e9Count % 2 == 1)
                            {
                                sh.Varname = "EGR aroREG_1 - Linearization map 2 [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwREG2KF";
                                sh.Z_axis_descr = "Maniputated variable (%)";
                                sh.Correction = 0.01;
                            }
                            if (e9Count % 2 == 0)
                            {
                                sh.Varname = "EGR aroREG_1 - Linearization map 1 [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwREG1KF";
                                sh.Z_axis_descr = "Manipulated variable (%)";
                                sh.Correction = 0.01;
                            }
                        }
                        else if ((sh.X_axis_ID / 256 == 0xC0 || sh.X_axis_ID == 0xE918) && sh.Y_axis_ID / 256 == 0xE9)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Ambient pressure (mBar)";
                            sh.YaxisUnits = "mbar";
                            sh.X_axis_descr = "Injection quantity (mg/stroke)";
                            sh.XaxisUnits = "mgst";
                            sh.X_axis_correction = 0.01;
                            sh.Varname = "EGR Pressure correction map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "arw2LM_KF";
                            sh.Z_axis_descr = "Correction (%)";
                            sh.Correction = 0.01;
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 4;
                            sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 12;
                        }
                        else if (sh.X_axis_ID / 256 == 0xEC && sh.Y_axis_ID / 256 == 0xC1)
                        {
                            int ecCount = GetMapNameCountForCodeBlock("EGR Engine temp correction - ", sh.CodeBlock, newSymbols, false);
                            ecCount--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.X_axis_descr = "Engine temperature (°C)";
                            sh.XaxisUnits = "°C";
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 12;
                            sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 4;
                            if (ecCount % 2 == 1)
                            {
                                sh.Varname = "EGR Engine temp correction - map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arw2TW_KF";
                                sh.Z_axis_descr = "Correction factor (%)";
                                sh.Correction = 0.01;
                            }
                            if (ecCount % 2 == 0)
                            {
                                sh.Varname = "EGR Engine temp correction - Target-value map (VP44) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwTWVEKF";
                                sh.Z_axis_descr = "Target air quantity (mg/stroke air)";
                                sh.Correction = 0.1;
                            }
                        }
                        else if (sh.X_axis_ID / 256 == 0xEC && (sh.Y_axis_ID / 256 == 0xC0 || sh.Y_axis_ID == 0xE918))
                        {
                            int ecCount = GetMapNameCountForCodeBlock("EGR Basic-", sh.CodeBlock, newSymbols, false);
                            ecCount--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.X_axis_descr = "Engine speed (rpm)";
                            sh.XaxisUnits = "rpm";
                            sh.Y_axis_descr = "Injected quantity (mg/stroke)";
                            sh.YaxisUnits = "mgst";
                            sh.Y_axis_correction = 0.01;
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 4;
                            sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 12;
                            if (ecCount % 2 == 1)
                            {
                                sh.Varname = "EGR Basic-Control for actuator 2 map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arw2ST_KF";
                                sh.Z_axis_descr = "Actuator Duty cycle (%)";
                                sh.Correction = 0.01;
                            }
                            if (ecCount % 2 == 0)
                            {
                                sh.Varname = "EGR Basic-Setpoint map (VP44) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "arwTWVEKF";
                                sh.Z_axis_descr = "Target air quantity (mg/stroke air)";
                                sh.Correction = 0.1;
                            }
                        }
                        else if (sh.X_axis_ID / 256 == 0xC1 && sh.Y_axis_ID / 256 == 0xC0)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Ambient pressure (mBar)";
                            sh.YaxisUnits = "mbar";
                            sh.X_axis_descr = "Ambient temperature (°C)";
                            sh.XaxisUnits = "°C";
                            sh.X_axis_correction = 0.1;
                            sh.X_axis_offset = -273.1;
                            sh.Varname = "Air mass calculation air temp/pressure correction map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Userdescription = "arwLMBKOKF";
                            sh.Z_axis_descr = "Correction factor";
                            sh.Correction = 0.0001;
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 4;
                            sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 12;
                        }
                        else if (sh.X_axis_ID / 256 == 0xC2 && sh.Y_axis_ID / 256 == 0xEC)
                        {
                            int c2Count = GetMapNameCountForCodeBlock("Torq limiter - ", sh.CodeBlock, newSymbols, false);
                            c2Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.X_axis_descr = "Speed/rpm ratio (km/h per rpm)";
                            sh.XaxisUnits = "km/h/rpm";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.X_axis_correction = 0.0000390625;
                            sh.X_axis_address = Convert.ToInt32(sh.Flash_start_address) - 4;
                            sh.Y_axis_address = Convert.ToInt32(sh.Flash_start_address) - 12;
                            if (c2Count % 2 == 1)
                            {
                                sh.Varname = "Torq limiter - Min map PI [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwADB2_KF";
                                sh.Z_axis_descr = "Torque limit (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                            if (c2Count % 2 == 0)
                            {
                                sh.Varname = "Torq limiter - Min map [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwADB_KF";
                                sh.Z_axis_descr = "Torque limit (mg/stroke)";
                                sh.Correction = 0.01;
                            }
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }

                    /*if (sh.X_axis_ID / 256 == 0xC1) // idle RPM
                    {
                        if (IsValidTemperatureAxis(allBytes, sh, MapViewerEx.AxisIdent.X_Axis))
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            int lmCount = GetMapNameCountForCodeBlock("Idle RPM", sh.CodeBlock, newSymbols, false);

                            sh.Varname = "Idle RPM (" + lmCount.ToString() + ") [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            sh.Y_axis_descr = "Coolant temperature";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Z_axis_descr = "Target engine speed";
                            sh.YaxisUnits = "°C";
                        }

                    }*/
                }
                else if (sh.Length == 6)
                {
                    if (sh.X_axis_length == 3 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID == 0xC19C || sh.X_axis_ID == 0xC168 || sh.X_axis_ID == 0xC1AC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Air temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Z_axis_descr = "Wait time (s)";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            sh.Correction = 0.01;
                            sh.Varname = "Glow intermediate glowing state time line (gswGS_T1ZG) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC270 || sh.X_axis_ID == 0xC234 || sh.X_axis_ID == 0xC27E)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Vehicle speed (km/h)";
                            sh.YaxisUnits = "km/h";
                            sh.Z_axis_descr = "Water temperature treshold (°C)";
                            sh.Y_axis_correction = 0.01;
                            sh.Offset = -273.1;
                            sh.Correction = 0.1;
                            sh.Varname = "AC Compressor water temperature switch-off limit line (klwWTab_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC19E || sh.X_axis_ID == 0xC16A || sh.X_axis_ID == 0xC1AE)
                        {
                            int c19eCount = GetMapNameCountForCodeBlock("Limiter ", sh.CodeBlock, newSymbols, false);
                            c19eCount--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Water temperature (°C)";
                            sh.YaxisUnits = "°C";
                            sh.Y_axis_correction = 0.1;
                            sh.Y_axis_offset = -273.1;
                            if (c19eCount % 2 == 1)
                            {
                                sh.Varname = "Limiter Smoke map selector (mrwBRA_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwBRA_KL";
                                sh.Z_axis_descr = "Selected map";
                                sh.Correction = 0.00390625;
                            }
                            if (c19eCount % 2 == 0)
                            {
                                sh.Varname = "Limiter Pre injection ramp-slope by water temperature (mrwVEBsLKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "mrwVEBsLKL";
                                sh.Z_axis_descr = "limit (mg/stroke per second)";
                                sh.Correction = 0.5;
                            }
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }
                }
                else if (sh.Length == 4)
                {
                    if (sh.X_axis_length == 2 && sh.Y_axis_length == 1)
                    {
                        if (sh.X_axis_ID == 0xEBA2 || sh.X_axis_ID == 0xEBA4 || sh.X_axis_ID == 0xE9BC)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "MAP linearization [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID / 256 == 0xC1) // idle RPM
                        {
                            if (IsValidTemperatureAxis(allBytes, sh, MapViewerEx.AxisIdent.X_Axis))
                            {
                                sh.Category = "Detected maps";
                                sh.Subcategory = "Misc";
                                int lmCount = GetMapNameCountForCodeBlock("Idle RPM", sh.CodeBlock, newSymbols, false);

                                sh.Varname = "Idle RPM (" + lmCount.ToString() + ") [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Y_axis_descr = "Coolant temperature";
                                sh.Y_axis_correction = 0.1;
                                sh.Y_axis_offset = -273.1;
                                sh.Z_axis_descr = "Target engine speed";
                                sh.YaxisUnits = "°C";
                            }

                        }
                        else if (sh.X_axis_ID == 0xEBC4 || sh.X_axis_ID == 0xEBC6)
                        {
                            if (IsValidVoltageAxis(allBytes, sh, MapViewerEx.AxisIdent.X_Axis))
                            {
                                sh.Category = "Detected maps";
                                sh.Subcategory = "Misc";
                                sh.Y_axis_descr = "Voltage (mV)";
                                sh.YaxisUnits = "mv";
                                sh.Z_axis_descr = "Oil Temperature (°C)";
                                sh.Y_axis_correction = 4.887585532747;
                                sh.Offset = -273.1;
                                sh.Correction = 0.1;
                                sh.Varname = "Oil temperature sensor linearization map (anwOTF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            }
                            else
                            {
                                sh.Category = "Detected maps";
                                sh.Subcategory = "Misc";
                                sh.Y_axis_descr = "Voltage (mV)";
                                sh.YaxisUnits = "mv";
                                sh.Y_axis_correction = 4.887585532747;
                                sh.Z_axis_descr = "Linearization (not active)";
                                sh.Varname = "Brake light switch linearization map (anwBRE_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                            }
                        }
                        else if (sh.X_axis_ID == 0xEC2E || sh.X_axis_ID == 0xEC2C || sh.X_axis_ID == 0xEC38)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.Z_axis_descr = "Atmospheric pressure (mBar)";
                            sh.Varname = "Atmospheric pressure correction linearization map (ldwLDBdPKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEBD8)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Raw duty cycle (%)";
                            sh.YaxisUnits = "%";
                            sh.Y_axis_correction = 0.01;
                            sh.Correction = 0.002;
                            sh.Z_axis_descr = "Refrigerant pressure (Bar)";
                            sh.Varname = "Refrigerant pressure linearization map (anwKMD_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEB9E || sh.X_axis_ID == 0xEBA0)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Voltage (mV)";
                            sh.YaxisUnits = "mv";
                            sh.Y_axis_correction = 4.887585532747;
                            sh.Z_axis_descr = "Linearization (not active)";
                            sh.Varname = "Analog/digital converter linearization map (anwTAD_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEBC2)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Voltage (mV)";
                            sh.YaxisUnits = "mv";
                            sh.Y_axis_correction = 4.887585532747;
                            sh.Z_axis_descr = "Linearization (not active)";
                            sh.Varname = "Brake light switch linearization map (anwBRE_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEB58 || sh.X_axis_ID == 0xEB5A)
                        {
                            int eb58Count = GetMapNameCountForCodeBlock("Voltage linearization map - ", sh.CodeBlock, newSymbols, false);
                            eb58Count--;
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Fixed values";
                            sh.YaxisUnits = "-";
                            if (eb58Count % 2 == 1)
                            {
                                sh.Varname = "Voltage linearization map - reference (not active) (anwREF_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "anwREF_KL";
                                sh.Z_axis_descr = "Linearized value";
                            }
                            if (eb58Count % 2 == 0)
                            {
                                sh.Varname = "Voltage linearization map - battery (not active) (anwUBAT_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                                sh.Userdescription = "anwUBAT_KL";
                                sh.Z_axis_descr = "Linearized value";
                            }
                        }
                        else if (sh.X_axis_ID == 0xEBA6 || sh.X_axis_ID == 0xEBA8)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Voltage (mV)";
                            sh.YaxisUnits = "mv";
                            sh.Y_axis_correction = 4.887585532747;
                            sh.Z_axis_descr = "Atmospheric pressure (mBar)";
                            sh.Varname = "Atmospheric pressure linearization map (anwBRE_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xEB10)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "fixed values";
                            sh.Z_axis_descr = "response";
                            sh.Varname = "Gate array response (edwGAR_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xF94A || sh.X_axis_ID == 0xF948)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Engine speed (rpm)";
                            sh.YaxisUnits = "rpm";
                            sh.Z_axis_descr = "Deactivation";
                            sh.Correction = 0.0001;
                            sh.Varname = "Deactivation of drag torque limitation due to red thrust monitor (mrwRSch_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xE920 || sh.X_axis_ID == 0xE926)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Injection quantity (mg/stroke)";
                            sh.YaxisUnits = "mgst";
                            sh.Z_axis_descr = "Corrected IQ (mg/stroke)";
                            sh.Correction = 0.01;
                            sh.Y_axis_correction = 0.01;
                            sh.Varname = "RME Fuel IQ correction line (arwRME_KL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        else if (sh.X_axis_ID == 0xC042 || sh.X_axis_ID == 0xC048)
                        {
                            sh.Category = "Detected maps";
                            sh.Subcategory = "Misc";
                            sh.Y_axis_descr = "Injection quantity (mg/stroke)";
                            sh.YaxisUnits = "mgst";
                            sh.Z_axis_descr = "Correction";
                            sh.Correction = 0.0001;
                            sh.Y_axis_correction = 0.01;
                            sh.Varname = "EGR Motor temperature correction pre-injection (VP44) path line (arwVEKORKL) [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";
                        }
                        /*else
                        {
                            sh.Category = "Debug detected maps";
                            sh.Subcategory = "Misc";
                            sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                        }*/
                    }
                }
                else if (sh.X_axis_ID == 0xDA6C && sh.Y_axis_ID == 0xDA6A)
                {
                    sh.Category = "Detected maps";
                    sh.X_axis_correction = 0.1;
                    sh.X_axis_offset = -273.1;
                    sh.XaxisUnits = "°C";
                    sh.Subcategory = "Limiters";
                    sh.Varname = "Boost correction by temperature [" + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";// " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_ID.ToString("X4") + " " + sh.Y_axis_ID.ToString("X4");
                    sh.X_axis_descr = "IAT (celcius)";
                    sh.Y_axis_descr = "Requested boost";
                    sh.Z_axis_descr = "Boost limit (mbar)";
                    sh.YaxisUnits = "mbar";
                }
                /*else
                {
                    sh.Category = "Debug detected maps";
                    sh.Subcategory = "Misc";
                    sh.Varname = "Map :" + sh.X_axis_length + " x " + sh.Y_axis_length + " IDs: X " + sh.X_axis_ID + " Y " + sh.Y_axis_ID + " Xr " + sh.X_axis_ID / 256 + " Yr " + sh.Y_axis_ID / 256 + " Len " + sh.Length + " XaM " + sh.X_axis_ID * 0xFFF0 + " YaM " + sh.Y_axis_ID * 0xFFF0 + " [ " + DetermineNumberByFlashBank(sh.Flash_start_address, newCodeBlocks) + "]";

                }*/
            }



        }

        private bool MapSelectorIndexEmpty(SymbolHelper sh)
        {
            bool retval = true;
            if (sh.MapSelector != null)
            {
                foreach (int iTest in sh.MapSelector.MapIndexes)
                {
                    if (iTest != 0) retval = false;
                }
            }
            return retval;
        }

        private int GetMaxAxisValue(byte[] allBytes, SymbolHelper sh, MapViewerEx.AxisIdent axisIdent)
        {
            int retval = 0;
            if (axisIdent == MapViewerEx.AxisIdent.X_Axis)
            {
                //read x axis values
                int offset = sh.X_axis_address;
                for (int i = 0; i < sh.X_axis_length; i++)
                {
                    int val = Convert.ToInt32(allBytes[offset]) + Convert.ToInt32(allBytes[offset + 1]) * 256;
                    if (val > retval) retval = val;
                    offset += 2;
                }
            }
            else if (axisIdent == MapViewerEx.AxisIdent.Y_Axis)
            {
                //read x axis values
                int offset = sh.Y_axis_address;
                for (int i = 0; i < sh.Y_axis_length; i++)
                {
                    int val = Convert.ToInt32(allBytes[offset]) + Convert.ToInt32(allBytes[offset + 1]) * 256;
                    if (val > retval) retval = val;
                    offset += 2;
                }
            }
            return retval;
        }

        private bool IsValidTemperatureAxis(byte[] allBytes, SymbolHelper sh, MapViewerEx.AxisIdent axisIdent)
        {
            bool retval = true;
            if (axisIdent == MapViewerEx.AxisIdent.X_Axis)
            {
                //read x axis values
                int offset = sh.X_axis_address;
                for (int i = 0; i < sh.X_axis_length; i++)
                {
                    int val = Convert.ToInt32(allBytes[offset]) + Convert.ToInt32(allBytes[offset + 1]) * 256;
                    double tempVal = (Convert.ToDouble(val) * 0.1) - 273.1;
                    if (tempVal < -80 || tempVal > 200) retval = false;
                    offset += 2;
                }
            }
            else if (axisIdent == MapViewerEx.AxisIdent.Y_Axis)
            {
                //read x axis values
                int offset = sh.Y_axis_address;
                for (int i = 0; i < sh.Y_axis_length; i++)
                {
                    int val = Convert.ToInt32(allBytes[offset]) + Convert.ToInt32(allBytes[offset + 1]) * 256;
                    double tempVal = (Convert.ToDouble(val) * 0.1) - 273.1;
                    if (tempVal < -80 || tempVal > 200) retval = false;
                    offset += 2;
                }
            }
            return retval;
        }

        private bool IsValidVoltageAxis(byte[] allBytes, SymbolHelper sh, MapViewerEx.AxisIdent axisIdent)
        {
            bool retval = true;
            if (axisIdent == MapViewerEx.AxisIdent.X_Axis)
            {
                //read x axis values
                int offset = sh.X_axis_address;
                for (int i = 0; i < sh.X_axis_length; i++)
                {
                    int val = Convert.ToInt32(allBytes[offset]) + Convert.ToInt32(allBytes[offset + 1]) * 256;
                    double tempVal = (Convert.ToDouble(val) * 4.887585532747);
                    if (tempVal < 10 || tempVal > 5500) retval = false;
                    offset += 2;
                }
            }
            else if (axisIdent == MapViewerEx.AxisIdent.Y_Axis)
            {
                //read x axis values
                int offset = sh.Y_axis_address;
                for (int i = 0; i < sh.Y_axis_length; i++)
                {
                    int val = Convert.ToInt32(allBytes[offset]) + Convert.ToInt32(allBytes[offset + 1]) * 256;
                    double tempVal = (Convert.ToDouble(val) * 4.887585532747);
                    if (tempVal < 10 || tempVal > 5500) retval = false;
                    offset += 2;
                }
            }
            return retval;
        }

        private double GetTemperatureDurRange(int index)
        {
            double retval = 0;
            // 
            return retval;
        }

        //SOI is selected on coolant temperature!
        private double GetTemperatureSOIRange(MapSelector sh, int index)
        {
            double retval = index;
            if (sh.MapData != null)
            {
                if (sh.MapData.Length > index)
                {
                    retval = (Convert.ToDouble(sh.MapData.GetValue(index)) * 0.1) - 273.1;
                }

            }
            return Math.Round(retval, 0);
        }

        private bool MapContainsNegativeValues(byte[] allBytes, SymbolHelper sh)
        {
            for (int i = 0; i < sh.Length; i += 2)
            {
                int currval = Convert.ToInt32(allBytes[sh.Flash_start_address + i + 1]) * 256 + Convert.ToInt32(allBytes[sh.Flash_start_address + i]);
                if (currval > 0xF000) return true;
            }
            return false;

        }

        private int GetMapNameCountForCodeBlock(string varName, int codeBlock, SymbolCollection newSymbols, bool debug)
        {
            int count = 0;
            if (debug) Console.WriteLine("Check " + varName + " " + codeBlock);

            foreach (SymbolHelper sh in newSymbols)
            {
                if (debug)
                {
                    if (!sh.Varname.StartsWith("2D") && !sh.Varname.StartsWith("3D"))
                    {
                        Console.WriteLine(sh.Varname + " " + sh.CodeBlock);
                    }
                }
                if (sh.Varname.StartsWith(varName) && sh.CodeBlock == codeBlock)
                {

                    if (debug) Console.WriteLine("Found " + sh.Varname + " " + sh.CodeBlock);

                    count++;
                }
            }
            count++;
            return count;
        }

        private int CheckCodeBlock(int offset, byte[] allBytes, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            int codeBlockID = 0;
            try
            {
                int endOfTable = Convert.ToInt32(allBytes[offset + 0x01000]) + Convert.ToInt32(allBytes[offset + 0x01001]) * 256 + offset;
                //sth wrong here with File 019AQ (ARL)
                int codeBlockAddress = Convert.ToInt32(allBytes[offset + 0x01002]) + Convert.ToInt32(allBytes[offset + 0x01003]) * 256 + offset;
                if (endOfTable == offset + 0xC3C3) return 0;               
                codeBlockID = Convert.ToInt32(allBytes[codeBlockAddress]) + Convert.ToInt32(allBytes[codeBlockAddress + 1]) * 256;
                //Why do we need line obove?
                //codeBlockID = Convert.ToInt32(allBytes[codeBlockAddress]);

                foreach (CodeBlock cb in newCodeBlocks)
                {
                    if (cb.StartAddress <= codeBlockAddress && cb.EndAddress >= codeBlockAddress)
                    {
                        cb.CodeID = codeBlockID;
                        cb.AddressID = codeBlockAddress;
                    }
                }
            }
            catch (Exception)
            {
            }
            return codeBlockID;
        }
        private bool AddToSymbolCollection(SymbolCollection newSymbols, SymbolHelper newSymbol, List<CodeBlock> newCodeBlocks)
        {
            if (newSymbol.Length >= 800) return false;
            foreach (SymbolHelper sh in newSymbols)
            {
                if (sh.Flash_start_address == newSymbol.Flash_start_address)
                {
                    //   Console.WriteLine("Already in collection: " + sh.Flash_start_address.ToString("X8"));
                    return false;
                }
                // not allowed to overlap
                /*else if (newSymbol.Flash_start_address > sh.Flash_start_address && newSymbol.Flash_start_address < (sh.Flash_start_address + sh.Length))
                {
                    Console.WriteLine("Overlapping map: " + sh.Flash_start_address.ToString("X8") + " " + sh.X_axis_length.ToString() + " x " + sh.Y_axis_length.ToString());
                    Console.WriteLine("Overlapping new: " + newSymbol.Flash_start_address.ToString("X8") + " " + newSymbol.X_axis_length.ToString() + " x " + newSymbol.Y_axis_length.ToString());
                    return false;
                }*/
            }
            newSymbols.Add(newSymbol);
            newSymbol.CodeBlock = DetermineCodeBlockByByAddress(newSymbol.Flash_start_address, newCodeBlocks);
            return true;
        }

        private bool isValidLength(int length, int id)
        {
            int idstrip = id / 256;
            if ((idstrip & 0xF0) == 0xE0) 
            //if (idstrip == 0xEB /*|| idstrip == 0xDE*/)
            {
                if (length > 0 && length <= 32) return true;
            }
            else
            {
                if (length > 0 && length < 32) return true;
            }
            //if (length <= 64) Console.WriteLine("seen id " + id.ToString("X4") + " with len " + length.ToString());

            return false;
        }

        private bool isAxisID(int id)
        {
            int idstrip = id / 256;
            if (idstrip == 0xDB) return true;
            if (idstrip == 0xC0 || idstrip == 0xC1 || idstrip == 0xC2 || idstrip == 0xC4 || idstrip == 0xC5) return true;
            if (idstrip == 0xE0 || idstrip == 0xE4 || idstrip == 0xE5 || idstrip == 0xE9 || idstrip == 0xEA || idstrip == 0xEB || idstrip == 0xEC) return true;
            if (idstrip == 0xDA /*|| idstrip == 0xDC */|| idstrip == 0xDD || idstrip == 0xDE) return true;
            if (idstrip == 0xF9 || idstrip == 0xFE) return true;
            if (idstrip == 0xE8) return true;
            //if (idstrip == 0xD7 || idstrip == 0xE6) return true;
           // if (idstrip == 0xD5) return true;
            return false;
        }

        // we need to check AHEAD for selector maps
        // if these are present we may be facing a complex map structure
        // which we need to handle in a special way (selectors always have data like 00 01 00 02 00 03 00 04 etc)
        private bool CheckMap(int t, byte[] allBytes, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks, out int len2Skip)
        {
            bool mapFound = false;
            bool retval = false;
            bool _dontGenMaps = false;
            len2Skip = 0;
            List<MapSelector> mapSelectors = new List<MapSelector>();
            if (t < allBytes.Length - 0x100)
            {
                /*if (t > 0x58000 && t < 0x60000)
                {
                    Console.WriteLine("Checkmap: " + t.ToString("X8"));
                }*/
                if (CheckAxisCount(t, allBytes, out mapSelectors) > 3)
                {
                    // check for selectors as well, and count them in the process
                    //Console.WriteLine("Offset " + t.ToString("X8") + " has more than 3 consecutive axis");
                    /*foreach (MapSelector ms in mapSelectors)
                    {
                        Console.WriteLine("selector: " + ms.StartAddress.ToString("X8") + " " + ms.MapLength.ToString() + " " + ms.NumRepeats.ToString());
                    }*/
                    _dontGenMaps = true;

                }

                int xaxisid = (Convert.ToInt32(allBytes[t + 1]) * 256) + Convert.ToInt32(allBytes[t]);

                if (isAxisID(xaxisid))
                {
                    int xaxislen = (Convert.ToInt32(allBytes[t + 3]) * 256) + Convert.ToInt32(allBytes[t + 2]);
                    // Console.WriteLine("Valid XID: " + xaxisid.ToString("X4") + " @" + t.ToString("X8") + " len: " + xaxislen.ToString("X2"));
                    if (isValidLength(xaxislen, xaxisid))
                    {
                        //Console.WriteLine("Valid XID: " + xaxisid.ToString("X4") + " @" + t.ToString("X8") + " len: " + xaxislen.ToString("X2"));
                        // misschien is er nog een as
                        int yaxisid = (Convert.ToInt32(allBytes[t + 5 + (xaxislen * 2)]) * 256) + Convert.ToInt32(allBytes[t + 4 + (xaxislen * 2)]);
                        int yaxislen = (Convert.ToInt32(allBytes[t + 7 + (xaxislen * 2)]) * 256) + Convert.ToInt32(allBytes[t + 6 + (xaxislen * 2)]);
                        if (isAxisID(yaxisid) && isValidLength(yaxislen, yaxisid))
                        {
                            // 3d map

                            int zaxisid = (Convert.ToInt32(allBytes[t + 9 + (xaxislen * 2) + (yaxislen * 2)]) * 256) + Convert.ToInt32(allBytes[t + 8 + (xaxislen * 2) + (yaxislen * 2)]);
                            //Console.WriteLine("Valid YID: " + yaxisid.ToString("X4") + " @" + t.ToString("X8") + " len: " + yaxislen.ToString("X2"));


                            //Console.WriteLine(t.ToString("X8") + " XID: " + xaxisid.ToString("X4") + " XLEN: " + xaxislen.ToString("X2") + " YID: " + yaxisid.ToString("X4") + " YLEN: " + yaxislen.ToString("X2"));
                            SymbolHelper newSymbol = new SymbolHelper();
                            newSymbol.X_axis_length = xaxislen;
                            newSymbol.Y_axis_length = yaxislen;
                            newSymbol.X_axis_ID = xaxisid;
                            newSymbol.Y_axis_ID = yaxisid;
                            newSymbol.X_axis_address = t + 4;
                            newSymbol.Y_axis_address = t + 8 + (xaxislen * 2);

                            newSymbol.Length = xaxislen * yaxislen * 2;
                            newSymbol.Flash_start_address = t + 8 + (xaxislen * 2) + (yaxislen * 2);
                            if (isAxisID(zaxisid))
                            {
                                int zaxislen = (Convert.ToInt32(allBytes[t + 11 + (xaxislen * 2) + (yaxislen * 2)]) * 256) + Convert.ToInt32(allBytes[t + 10 + (xaxislen * 2) + (yaxislen * 2)]);

                                int zaxisaddress = t + 12 + (xaxislen * 2) + (yaxislen * 2);

                                if (isValidLength(zaxislen, zaxisid))
                                {
                                    //   newSymbol.Flash_start_address += 0x10; // dan altijd 16 erbij
                                    int len2skip = (4 + zaxislen * 2);
                                    if (len2skip < 16) len2skip = 16; // at least 16 bytes
                                    newSymbol.Flash_start_address += len2skip;

                                    len2Skip += (xaxislen * 2) + (yaxislen * 2) + zaxislen * 2;

                                    if (!_dontGenMaps)
                                    {
                                        // this has something to do with repeating several times with the same axis set

                                        Console.WriteLine("Added " + len2skip.ToString() + " because of z axis " + newSymbol.Flash_start_address.ToString("X8"));


                                        // maybe there are multiple maps between the end of the map and the start of the next axis
                                        int nextMapAddress = findNextMap(allBytes, (int)(newSymbol.Flash_start_address + newSymbol.Length), newSymbol.Length * 10);
                                        if (nextMapAddress > 0)
                                        {
                                            // is it divisable by the maplength

                                            if (((nextMapAddress - newSymbol.Flash_start_address) % newSymbol.Length) == 0)
                                            {

                                                int numberOfrepeats = (int)(nextMapAddress - newSymbol.Flash_start_address) / newSymbol.Length;
                                                numberOfrepeats = zaxislen;
                                                if (numberOfrepeats > 1)
                                                {
                                                    MapSelector ms = new MapSelector();
                                                    ms.NumRepeats = numberOfrepeats;
                                                    ms.MapLength = newSymbol.Length;
                                                    ms.StartAddress = zaxisaddress;
                                                    ms.XAxisAddress = newSymbol.X_axis_address;
                                                    ms.YAxisAddress = newSymbol.Y_axis_address;
                                                    ms.XAxisLen = newSymbol.X_axis_length;
                                                    ms.YAxisLen = newSymbol.Y_axis_length;
                                                    ms.MapData = new int[zaxislen];
                                                    int boffset = 0;
                                                    for (int ia = 0; ia < zaxislen; ia++)
                                                    {
                                                        int axisValue = Convert.ToInt32(allBytes[zaxisaddress + boffset]) + Convert.ToInt32(allBytes[zaxisaddress + boffset + 1]) * 256;
                                                        ms.MapData.SetValue(axisValue, ia);
                                                        boffset += 2;
                                                    }

                                                    ms.MapIndexes = new int[zaxislen];
                                                    for (int ia = 0; ia < zaxislen; ia++)
                                                    {
                                                        int axisValue = Convert.ToInt32(allBytes[zaxisaddress + boffset]) + Convert.ToInt32(allBytes[zaxisaddress + boffset + 1]) * 256;
                                                        ms.MapIndexes.SetValue(axisValue, ia);
                                                        boffset += 2;
                                                    }

                                                    // numberOfrepeats--;
                                                    //int idx = 0;

                                                    for (int maprepeat = 0; maprepeat < numberOfrepeats; maprepeat++)
                                                    {
                                                        // idx ++;
                                                        SymbolHelper newGenSym = new SymbolHelper();
                                                        newGenSym.X_axis_length = newSymbol.X_axis_length;
                                                        newGenSym.Y_axis_length = newSymbol.Y_axis_length;
                                                        newGenSym.X_axis_ID = newSymbol.X_axis_ID;
                                                        newGenSym.Y_axis_ID = newSymbol.Y_axis_ID;
                                                        newGenSym.X_axis_address = newSymbol.X_axis_address;
                                                        newGenSym.Y_axis_address = newSymbol.Y_axis_address;
                                                        newGenSym.Flash_start_address = newSymbol.Flash_start_address + maprepeat * newSymbol.Length;
                                                        newGenSym.Length = newSymbol.Length;
                                                        newGenSym.Varname = "3D GEN " + newGenSym.Flash_start_address.ToString("X8") + " " + xaxisid.ToString("X4") + " " + yaxisid.ToString("X4");
                                                        newGenSym.MapSelector = ms;
                                                        // attach a mapselector to these maps
                                                        // only add it if the map is not empty
                                                        // otherwise we will cause confusion among users
                                                        if (maprepeat > 0)
                                                        {
                                                            try
                                                            {
                                                                if (ms.MapIndexes[maprepeat] > 0)
                                                                {
                                                                    retval = AddToSymbolCollection(newSymbols, newGenSym, newCodeBlocks);
                                                                    if (retval)
                                                                    {
                                                                        mapFound = true;
                                                                        //GUIDO len2Skip += newGenSym.Length;
                                                                        //t += (xaxislen * 2) + (yaxislen * 2) + newGenSym.Length;
                                                                    }
                                                                }
                                                            }
                                                            catch (Exception)
                                                            {
                                                            }
                                                        }
                                                        else
                                                        {
                                                            retval = AddToSymbolCollection(newSymbols, newGenSym, newCodeBlocks);
                                                            if (retval)
                                                            {
                                                                mapFound = true;
                                                                //GUIDO len2Skip += (xaxislen * 2) + (yaxislen * 2) + newGenSym.Length;
                                                                //t += (xaxislen * 2) + (yaxislen * 2) + newGenSym.Length;
                                                            }
                                                        }
                                                    }
                                                }
                                                //Console.WriteLine("Indeed!");
                                                // the first one will be added anyway.. add the second to the last

                                            }

                                        }
                                    }
                                    else
                                    {

                                        int maxisid = (Convert.ToInt32(allBytes[t + 13 + (xaxislen * 2) + (yaxislen * 2) + (zaxislen * 2)]) * 256) + Convert.ToInt32(allBytes[t + 12 + (xaxislen * 2) + (yaxislen * 2) + zaxislen * 2]);
                                        int maxislen = (Convert.ToInt32(allBytes[t + 15 + (xaxislen * 2) + (yaxislen * 2) + (zaxislen * 2)]) * 256) + Convert.ToInt32(allBytes[t + 14 + (xaxislen * 2) + (yaxislen * 2) + zaxislen * 2]);
                                        //maxislen *= 2;

                                        int maxisaddress = t + 16 + (xaxislen * 2) + (yaxislen * 2);

                                        if (isAxisID(maxisid))
                                        {
                                            newSymbol.Flash_start_address += (maxislen * 2) + 4;
                                        }
                                        // special situation, handle selectors
                                        //Console.WriteLine("Map start address = " + newSymbol.Flash_start_address.ToString("X8"));
                                        long lastFlashAddress = newSymbol.Flash_start_address;
                                        foreach (MapSelector ms in mapSelectors)
                                        {

                                            // check the memory size between the start of the map and the 
                                            // start of the map selector
                                            long memsize = ms.StartAddress - lastFlashAddress;
                                            memsize /= 2; // in words
                                            if (ms.NumRepeats > 0)
                                            {
                                                int mapsize = Convert.ToInt32(memsize) / ms.NumRepeats;
                                                if ((xaxislen * yaxislen) == mapsize)
                                                {
                                                    //Console.WriteLine("selector: " + ms.StartAddress.ToString("X8") + " " + ms.MapLength.ToString() + " " + ms.NumRepeats.ToString());
                                                    //Console.WriteLine("memsize = " + memsize.ToString() + " mapsize " + mapsize.ToString());
                                                    //Console.WriteLine("starting at address: " + lastFlashAddress.ToString("X8"));
                                                    // first axis set
                                                    //len2Skip += (xaxislen * 2) + (yaxislen * 2);
                                                    for (int i = 0; i < ms.NumRepeats; i++)
                                                    {
                                                        SymbolHelper shGen2 = new SymbolHelper();
                                                        shGen2.MapSelector = ms;
                                                        shGen2.X_axis_length = newSymbol.X_axis_length;
                                                        shGen2.Y_axis_length = newSymbol.Y_axis_length;
                                                        shGen2.X_axis_ID = newSymbol.X_axis_ID;
                                                        shGen2.Y_axis_ID = newSymbol.Y_axis_ID;
                                                        shGen2.X_axis_address = newSymbol.X_axis_address;
                                                        shGen2.Y_axis_address = newSymbol.Y_axis_address;
                                                        shGen2.Length = mapsize * 2;
                                                        //shGen2.Category = "Generated";
                                                        long address = lastFlashAddress;
                                                        shGen2.Flash_start_address = address;
                                                        //shGen2.Correction = 0.023437; // TEST
                                                        //shGen2.Varname = "Generated* " + shGen2.Flash_start_address.ToString("X8") + " " + ms.StartAddress.ToString("X8") + " " + ms.NumRepeats.ToString() + " " + i.ToString();
                                                        shGen2.Varname = "3D " + shGen2.Flash_start_address.ToString("X8") + " " + shGen2.X_axis_ID.ToString("X4") + " " + shGen2.Y_axis_ID.ToString("X4");
                                                        //if (i < ms.NumRepeats - 1)
                                                        {
                                                            retval = AddToSymbolCollection(newSymbols, shGen2, newCodeBlocks);
                                                            if (retval)
                                                            {
                                                                mapFound = true;
                                                                //GUIDO len2Skip += shGen2.Length;
                                                                //t += (xaxislen * 2) + (yaxislen * 2) + shGen2.Length;
                                                            }
                                                        }
                                                        lastFlashAddress = address + mapsize * 2;
                                                        // Console.WriteLine("Set last address to " + lastFlashAddress.ToString("X8"));
                                                    }
                                                    lastFlashAddress += ms.NumRepeats * 4 + 4;
                                                }
                                                else if ((zaxislen * maxislen) == mapsize)
                                                {
                                                    // second axis set
                                                   // len2Skip += (xaxislen * 2) + (yaxislen * 2);
                                                    for (int i = 0; i < ms.NumRepeats; i++)
                                                    {
                                                        SymbolHelper shGen2 = new SymbolHelper();
                                                        shGen2.MapSelector = ms;
                                                        shGen2.X_axis_length = maxislen;
                                                        shGen2.Y_axis_length = zaxislen;
                                                        shGen2.X_axis_ID = maxisid;
                                                        shGen2.Y_axis_ID = zaxisid;
                                                        shGen2.X_axis_address = maxisaddress;
                                                        shGen2.Y_axis_address = zaxisaddress;
                                                        shGen2.Length = mapsize * 2;
                                                        //shGen2.Category = "Generated";
                                                        long address = lastFlashAddress;
                                                        shGen2.Flash_start_address = address;
                                                        //shGen2.Varname = "Generated** " + shGen2.Flash_start_address.ToString("X8");
                                                        shGen2.Varname = "3D " + shGen2.Flash_start_address.ToString("X8") + " " + shGen2.X_axis_ID.ToString("X4") + " " + shGen2.Y_axis_ID.ToString("X4");

                                                        //if (i < ms.NumRepeats - 1)
                                                        {
                                                            retval = AddToSymbolCollection(newSymbols, shGen2, newCodeBlocks);
                                                            if (retval)
                                                            {
                                                                mapFound = true;
                                                                //GUIDO len2Skip += shGen2.Length;
                                                                //t += (xaxislen * 2) + (yaxislen * 2) + shGen2.Length;
                                                            }
                                                        }
                                                        lastFlashAddress = address + mapsize * 2;
                                                        //Console.WriteLine("Set last address 2 to " + lastFlashAddress.ToString("X8"));
                                                    }
                                                    lastFlashAddress += ms.NumRepeats * 4 + 4;
                                                }
                                            }
                                            //if(ms.NumRepeats

                                        }
                                    }
                                }
                            }

                            newSymbol.Varname = "3D Map Size: " + newSymbol.X_axis_length + "x" + newSymbol.Y_axis_length + " Loc: " + newSymbol.Flash_start_address.ToString("X6") + " IDs: X " + newSymbol.X_axis_ID.ToString("X4") + " Y " + newSymbol.Y_axis_ID.ToString("X4") + " Xr " + (newSymbol.X_axis_ID / 256).ToString("X2") + " Yr " + (newSymbol.Y_axis_ID / 256).ToString("X2") + " Len: " + newSymbol.Length + " Cb: [" + DetermineNumberByFlashBank(newSymbol.Flash_start_address, newCodeBlocks) + "]";
                            //Console.WriteLine(newSymbol.Varname + " " + newSymbol.Length.ToString() + " " + newSymbol.X_axis_length.ToString() + "x" + newSymbol.Y_axis_length.ToString());
                            retval = AddToSymbolCollection(newSymbols, newSymbol, newCodeBlocks);
                            if (retval)
                            {
                                mapFound = true;
                                //GUIDO len2Skip += (xaxislen * 2) + (yaxislen * 2) + newSymbol.Length;
                                //t += (xaxislen * 2) + (yaxislen * 2) + newSymbol.Length;
                            }

                        }
                        else
                        {
                            if (yaxisid > 0xC000 && yaxisid < 0xF000 && yaxislen <= 32) Console.WriteLine("Unknown map id: " + yaxisid.ToString("X4") + " len " + yaxislen.ToString("X4") + " at address " + t.ToString("X8"));
                            SymbolHelper newSymbol = new SymbolHelper();
                            newSymbol.X_axis_length = xaxislen;
                            newSymbol.X_axis_ID = xaxisid;
                            newSymbol.X_axis_address = t + 4;
                            newSymbol.Length = xaxislen * 2;
                            newSymbol.Flash_start_address = t + 4 + (xaxislen * 2);
                            newSymbol.Varname = "2D Map Size: " + newSymbol.X_axis_length + "x" + newSymbol.Y_axis_length + " Loc: " + newSymbol.Flash_start_address.ToString("X6") + " IDs: X " + newSymbol.X_axis_ID.ToString("X4") + " Y " + newSymbol.Y_axis_ID.ToString("X4") + " Xr " + (newSymbol.X_axis_ID / 256).ToString("X2") + " Yr " + (newSymbol.Y_axis_ID / 256).ToString("X2") + " Len: " + newSymbol.Length + " Cb: [" + DetermineNumberByFlashBank(newSymbol.Flash_start_address, newCodeBlocks) + "]";
                            //newSymbols.Add(newSymbol);
                            newSymbol.CodeBlock = DetermineCodeBlockByByAddress(newSymbol.Flash_start_address, newCodeBlocks);
                            retval = AddToSymbolCollection(newSymbols, newSymbol, newCodeBlocks);
                            if (retval)
                            {
                                mapFound = true;
                                //GUIDO len2Skip += (xaxislen * 2);
                                //t += (xaxislen * 2);
                            }
                            // 2d map
                        }
                    }

                }
            }
            return mapFound;
        }

        private bool MapIsEmpty(byte[] allBytes, SymbolHelper sh)
        {
            for (int i = 0; i < sh.Length; i += 2)
            {
                int currval = Convert.ToInt32(allBytes[sh.Flash_start_address + i + 1]) * 256 + Convert.ToInt32(allBytes[sh.Flash_start_address + i]);
                if (currval != 0) return false;
            }
            return true;
        }

        private int findNextMap(byte[] allBytes, int index, int maxBytesToSearch)
        {
            int retval = 0;
            for (int i = index; i < index + maxBytesToSearch; i += 2)
            {
                int xaxisid = (Convert.ToInt32(allBytes[i + 1]) * 256) + Convert.ToInt32(allBytes[i]);
                if (isAxisID(xaxisid))
                {
                    int xaxislen = (Convert.ToInt32(allBytes[i + 3]) * 256) + Convert.ToInt32(allBytes[i + 2]);
                    if (isValidLength(xaxislen, xaxisid))
                    {
                        return i;
                    }
                }
            }
            return retval;
        }

        private int CheckAxisCount(int offset, byte[] allBytes, out List<MapSelector> mapSelectors)
        {
            int axisCount = 0;
            /*if (offset == 0x58504)
            {
                Console.WriteLine("58504");
            }*/
            mapSelectors = new List<MapSelector>();
            bool axisFound = true;
            int t = offset;
            while (axisFound)
            {
                axisFound = false;
                int axisid = (Convert.ToInt32(allBytes[t + 1]) * 256) + Convert.ToInt32(allBytes[t]);
                if (isAxisID(axisid))
                {
                    int axislen = (Convert.ToInt32(allBytes[t + 3]) * 256) + Convert.ToInt32(allBytes[t + 2]);
                    if (axislen > 0 && axislen < 32)
                    {
                        axisCount++;
                        axisFound = true;
                        t += 4 + (axislen * 2);
                    }

                }
            }
            // search from offset 't' for selectors
            // maximum searchrange = 0x1000
            int BytesToSearch = 5120 + 16;
            if (axisCount > 3)
            {
                while (BytesToSearch > 0)
                {
                    int axisid = (Convert.ToInt32(allBytes[t + 1]) * 256) + Convert.ToInt32(allBytes[t]);
                    if (isAxisID(axisid))
                    {
                        //Console.WriteLine("Checking address: " + t.ToString("X8"));
                        int axislen = (Convert.ToInt32(allBytes[t + 3]) * 256) + Convert.ToInt32(allBytes[t + 2]);
                        if (axislen <= 10) // more is not valid for selectors
                        {
                            // read & verify data (00 00 00 01 00 02 00 03 etc)
                            bool selectorValid = true;
                            int num = 0;
                            uint prevSelector = 0;
                            for (int i = 0; i < (axislen * 2); i += 2)
                            {
                                uint selValue = Convert.ToUInt32(allBytes[t + 4 + (axislen * 2) + i]) + Convert.ToUInt32(allBytes[t + 4 + (axislen * 2) + 1 + i]);
                                //Console.WriteLine("Selval: " + selValue.ToString() + " num: " + num.ToString());
                                /*if (axislen < 3)
                                {
                                    selectorValid = false;
                                    break;
                                }*/
                                if (allBytes[t + 4 + (axislen * 2) + i] != 0)
                                {
                                    if(allBytes[t + 4 + (axislen * 2) + i] != 0x40) selectorValid = false;
                                    break;
                                }
                                if (allBytes[t + 4 + (axislen * 2) + 1 + i] > 9)
                                {
                                    selectorValid = false;
                                    break;
                                }
                                if (prevSelector > selValue)
                                {
                                    selectorValid = false;
                                    break;
                                }
                                prevSelector = selValue;
                                /*if (num != selValue)
                                {
                                    // not a valid selector
                                    selectorValid = false;
                                    break;
                                }*/
                                num++;
                            }
                            if (selectorValid)
                            {
                                // create a new selector
                                //Console.WriteLine("Selector valid " + t.ToString("X8"));
                                MapSelector newSel = new MapSelector();
                                newSel.NumRepeats = axislen;
                                newSel.StartAddress = t;

                                // read the data into the mapselector
                                newSel.MapData = new int[axislen];
                                int boffset = 0;
                                for (int ia = 0; ia < axislen; ia++)
                                {
                                    int axisValue = Convert.ToInt32(allBytes[newSel.StartAddress + 4 + boffset]) + Convert.ToInt32(allBytes[newSel.StartAddress + 4 + boffset + 1]) * 256;
                                    newSel.MapData.SetValue(axisValue, ia);
                                    boffset += 2;
                                }
                                mapSelectors.Add(newSel);
                                if (mapSelectors.Count > 5) break;

                                BytesToSearch = 5120 + 16;
                            }
                        }
                    }
                    t += 2;
                    BytesToSearch -= 2;
                }
            }
            return axisCount;
        }

        private void VerifyCodeBlocks(byte[] allBytes, SymbolCollection newSymbols, List<CodeBlock> newCodeBlocks)
        {
            //000001=automatic,000002=manual,000003=4wd ????
            Tools.Instance.m_codeBlock5ID = 0;
            Tools.Instance.m_codeBlock6ID = 0;
            Tools.Instance.m_codeBlock7ID = 0;
            bool found = true;
            int offset = 0;
            int defaultCodeBlockLength = 0x10000;
            int currentCodeBlockLength = 0;
            int prevCodeBlockStart = 0;
            while (found)
            {
                int CodeBlockAddress = Tools.Instance.findSequence(allBytes, offset, new byte[11] { 0xC1, 0x02, 0x00, 0x68, 0x00, 0x25, 0x03, 0x00, 0x00, 0x10, 0x27 }, new byte[11] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
                if (CodeBlockAddress > 0)
                {
                    CodeBlock newcodeblock = new CodeBlock();
                    newcodeblock.StartAddress = CodeBlockAddress - 1;
                    if (prevCodeBlockStart == 0) prevCodeBlockStart = newcodeblock.StartAddress;
                    else if (currentCodeBlockLength == 0)
                    {
                        currentCodeBlockLength = newcodeblock.StartAddress - prevCodeBlockStart;
                        if (currentCodeBlockLength > 0x10000) currentCodeBlockLength = 0x10000;
                    }
                    // find the next occurence of the checksum
                    newCodeBlocks.Add(newcodeblock);
                    offset = CodeBlockAddress + 1;
                }
                else found = false;
            }
            foreach (CodeBlock cb in newCodeBlocks)
            {
                if (currentCodeBlockLength != 0) cb.EndAddress = cb.StartAddress + currentCodeBlockLength - 1;
                else cb.EndAddress = cb.StartAddress + defaultCodeBlockLength - 1;
            }
            foreach (CodeBlock cb in newCodeBlocks)
            {
                int autoSequenceIndex = Tools.Instance.findSequence(allBytes, cb.StartAddress, new byte[7] { 0x45, 0x44, 0x43, 0x20, 0x20, 0x41, 0x47 }, new byte[7] { 1, 1, 1, 1, 1, 1, 1 });
                int manualSequenceIndex = Tools.Instance.findSequence(allBytes, cb.StartAddress, new byte[7] { 0x45, 0x44, 0x43, 0x20, 0x20, 0x53, 0x47 }, new byte[7] { 1, 1, 1, 1, 1, 1, 1 });
                if (autoSequenceIndex < cb.EndAddress && autoSequenceIndex >= cb.StartAddress) cb.BlockGearboxType = GearboxType.Automatic;
                if (manualSequenceIndex < cb.EndAddress && manualSequenceIndex >= cb.StartAddress) cb.BlockGearboxType = GearboxType.Manual;
            }
            if (Tools.Instance.m_currentfilelength >= 0x80000)
            {
                Tools.Instance.m_codeBlock5ID = CheckCodeBlock(0x50000, allBytes, newSymbols, newCodeBlocks); //manual specific
                //File ARL 019AQ -> CodeBlock ID=5882 appered?
                Tools.Instance.m_codeBlock6ID = CheckCodeBlock(0x60000, allBytes, newSymbols, newCodeBlocks); //automatic specific
                Tools.Instance.m_codeBlock7ID = CheckCodeBlock(0x70000, allBytes, newSymbols, newCodeBlocks); //quattro specific
            }
        }
    }
}
