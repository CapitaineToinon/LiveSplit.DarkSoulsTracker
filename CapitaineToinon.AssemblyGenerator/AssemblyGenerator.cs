using System;
using System.Collections.Generic;
using System.Linq;

namespace CapitaineToinon
{
    public class AssemblyGenerator
    {
        private byte[] bytes;
        public Int32 pos;

        public byte[] Bytes { get => bytes; }

        private Dictionary<string, IntPtr> reg8;
        private Dictionary<string, IntPtr> reg16;
        private Dictionary<string, IntPtr> reg32;
        private Dictionary<string, IntPtr> code;
        private Dictionary<string, IntPtr> vars;

        private SortedList<int, string> varrefs;

        public AssemblyGenerator()
        {
            bytes = new byte[] { };
            pos = 0;
            reg8 = new Dictionary<string, IntPtr>();
            reg16 = new Dictionary<string, IntPtr>();
            reg32 = new Dictionary<string, IntPtr>();
            code = new Dictionary<string, IntPtr>();
            vars = new Dictionary<string, IntPtr>();
            varrefs = new SortedList<int, string>();
            init();
        }

        public void Clear()
        {
            bytes = new byte[] { };
            vars.Clear();
            varrefs.Clear();
            pos = 0;
        }

        private void init()
        {
            reg8.Clear();
            reg8.Add("al", (IntPtr)0);
            reg8.Add("cl", (IntPtr)1);
            reg8.Add("dl", (IntPtr)2);
            reg8.Add("bl", (IntPtr)3);
            reg8.Add("ah", (IntPtr)4);
            reg8.Add("ch", (IntPtr)5);
            reg8.Add("dh", (IntPtr)6);
            reg8.Add("bh", (IntPtr)7);

            reg16.Clear();
            reg16.Add("ax", (IntPtr)0);
            reg16.Add("cx", (IntPtr)1);
            reg16.Add("dx", (IntPtr)2);
            reg16.Add("bx", (IntPtr)3);

            reg32.Clear();
            reg32.Add("eax", (IntPtr)0);
            reg32.Add("ecx", (IntPtr)1);
            reg32.Add("edx", (IntPtr)2);
            reg32.Add("ebx", (IntPtr)3);
            reg32.Add("esp", (IntPtr)4);
            reg32.Add("ebp", (IntPtr)5);
            reg32.Add("esi", (IntPtr)6);
            reg32.Add("edi", (IntPtr)7);

            code.Clear();
            code.Add("inc", (IntPtr)0x40);
            code.Add("dec", (IntPtr)0x48);
            //code.Add("push", &H50)
            code.Add("pop", (IntPtr)0x58);
            code.Add("pushad", (IntPtr)0x60);
            code.Add("popad", (IntPtr)0x61);
        }

        public void Add(byte[] newbytes)
        {
            bytes = bytes.Concat(newbytes).ToArray();
        }

        public void AddVar(string name, string hexval)
        {
            AddVar(name, Convert.ToInt32(hexval.Substring(2)));
        }

        public void AddVar(string name, IntPtr val)
        {
            AddVar(name, (int)val);
        }

        public void AddVar(string name, Int32 val)
        {
            name = name.Replace(":", "");

            if (!vars.ContainsKey(name))
            {
                vars.Add(name, (IntPtr)val);
            }
            else
            {
                vars[name] = (IntPtr)val;
                foreach (KeyValuePair<int, string> keyPair in varrefs)
                {
                    if (keyPair.Value == name)
                    {
                        byte[] tmpbyt = new byte[] { };

                        switch (bytes[keyPair.Key])
                        {
                            case 0xE8:
                            case 0xE9:
                                tmpbyt = BitConverter.GetBytes(val - (pos - (bytes.Length - keyPair.Key)) - 5);
                                Array.Copy(tmpbyt, 0, bytes, keyPair.Key + 1, tmpbyt.Length);
                                break;
                            case 0xF:
                                tmpbyt = BitConverter.GetBytes(val - (pos - (bytes.Length - keyPair.Key)) - 6);
                                Array.Copy(tmpbyt, 0, bytes, keyPair.Key + 2, tmpbyt.Length);
                                break;
                        }
                    }
                }
            }
        }

        private void ParseInput(string str,
                                ref string cmd,
                                ref string reg1, ref string reg2,
                                ref bool ptr1, ref bool ptr2,
                                ref Int32 plus1, ref Int32 plus2,
                                ref Int32 val1, ref Int32 val2)
        {
            // Raw parameters
            string AllParam = "";
            string param1 = "";
            string param2 = "";

            // Seperate command from params
            if (str.Contains(" "))
            {
                cmd = str.Split(' ')[0];
                AllParam = str.Substring(cmd.Length);
                AllParam = AllParam.Replace(" ", "");
            }
            else
            {
                cmd = str;
            }

            // Check for section names
            if (cmd.Contains(":"))
            {
                AddVar(cmd, pos);
                return;
            }

            // Splits params
            if (AllParam.Contains(","))
            {
                param2 = AllParam.Split(',')[1];
            }
            param1 = AllParam.Split(',')[0];

            // Check if immediate or pointers
            if (param1.Contains("["))
            {
                ptr1 = true;
                param1 = param1.Replace("[", "");
                param1 = param1.Replace("]", "");
            }

            if (param2.Contains("["))
            {
                ptr2 = true;
                param2 = param2.Replace("[", "");
                param2 = param2.Replace("]", "");
            }

            // Check if there are offsets in params
            if ((param1.Contains("+") || param1.Contains("-")))
            {
                if (param1.Contains("0x"))
                {
                    plus1 = Convert.ToInt32((param1[3] + param1.Substring((param1.Length - (param1.Length - 6)))), 16);
                }
                else
                {
                    plus1 = Convert.ToInt32((param1[3] + param1.Substring((param1.Length - (param1.Length - 4)))));
                }

                param1 = param1.Split('+')[0];
                param1 = param1.Split('-')[0];
            }

            if ((param2.Contains("+") || param2.Contains("-")))
            {
                if (param2.Contains("0x"))
                {
                    // plus2 = Convert.ToInt32(param2(3) & Microsoft.VisualBasic.Right(param2, param2.Length - 6), 16)
                    plus2 = Convert.ToInt32(param2.Substring((param2.Length - (param2.Length - 4))), 16);
                    if ((param2[3] == '-'))
                    {
                        plus2 *= -1;
                    }
                }
                else
                {
                    plus2 = Convert.ToInt32((param2[3] + param2.Substring((param2.Length - (param2.Length - 4)))));
                }

                param2 = param2.Split('+')[0];
                param2 = param2.Split('-')[0];
            }

            // If not registers, convert params from hex to dec
            if (param1.Contains("0x"))
            {
                val1 = Convert.ToInt32(param1, 16);
            }

            if (param2.Contains("0x"))
            {
                val2 = Convert.ToInt32(param2, 16);
            }

            int tryparse;
            if (Int32.TryParse(param1, out tryparse))
            {
                val1 = tryparse;
            }
            if (Int32.TryParse(param2, out tryparse))
            {
                val2 = tryparse;
            }

            // Define registers, if not values
            if (reg32.ContainsKey(param1))
                reg1 = param1;
            if (reg32.ContainsKey(param2))
                reg2 = param2;
            if (reg16.ContainsKey(param1))
                reg1 = param1;
            if (reg16.ContainsKey(param2))
                reg2 = param2;
            if (reg8.ContainsKey(param1))
                reg1 = param1;
            if (reg8.ContainsKey(param2))
                reg2 = param2;

            // If param is previously defined section
            if (vars.ContainsKey(param1))
            {
                val1 = (int)vars[param1];
                varrefs.Add(bytes.Length, param1);
            }

            if (vars.ContainsKey(param2))
            {
                val2 = (int)vars[param2];
                varrefs.Add(bytes.Length, param2);
            }
        }

        public void Asm(string str)
        {
            string cmd = "";
            string reg1 = "";
            string reg2 = "";
            bool ptr1 = false;
            bool ptr2 = false;
            Int32 plus1 = 0;
            Int32 plus2 = 0;
            Int32 val1 = 0;
            Int32 val2 = 0;

            ParseInput(str, ref cmd, ref reg1, ref reg2, ref ptr1, ref ptr2, ref plus1, ref plus2, ref val1, ref val2);

            byte[] newbytes = new byte[] { };
            int addr;

            // Check if command is simple 1-byte command
            if (code.ContainsKey(cmd))
            {
                newbytes = new byte[] { 0 };
                newbytes[0] = (byte)code[cmd];
                if (reg32.ContainsKey(reg1))
                {
                    newbytes[0] = (byte)(newbytes[0] | (int)reg32[reg1]);
                }

                Add(newbytes);
                pos += newbytes.Length;
                return;
            }

            switch (cmd)
            {
                case "add":
                    if ((reg32.ContainsKey(reg1) && (reg2 == "")))
                    {
                        newbytes = new byte[] { 0x81, 0xC0 };
                        if ((Math.Abs(val2) < 0x80))
                        {
                            newbytes[0] = (byte)(newbytes[0] | 2);
                            newbytes = newbytes.Concat(new byte[] { (byte)(val2 & 0xFF) }).ToArray();
                        }
                        else
                        {
                            if ((reg1 == "eax"))
                            {
                                newbytes = new byte[] { 5 };
                            }
                            newbytes = newbytes.Concat(BitConverter.GetBytes(val2)).ToArray();
                        }
                        newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                    }

                    if (reg32.ContainsKey(reg1) && reg32.ContainsKey(reg2))
                    {
                        newbytes = new byte[] { 1, 0 };

                        if (ptr1)
                        {
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg2] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                        }
                        if (ptr2)
                        {
                            newbytes[0] = (byte)(newbytes[0] | 2);
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg1] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg2]);
                        }

                        if (!(ptr1 || ptr2))
                        {
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg2] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                            newbytes[1] = (byte)(newbytes[1] | 0xC0);
                        }

                        int offset;
                        offset = plus1 + plus2;

                        if ((Math.Abs(offset) < 0x80))
                        {
                            if ((offset > 0))
                            {
                                newbytes[1] = (byte)(newbytes[1] | 0x40);
                                newbytes = newbytes.Concat(new byte[] { (byte)(offset & 0xFF) }).ToArray();
                            }
                        }
                        if ((Math.Abs(offset) > 0x7F))
                        {
                            newbytes[1] = (byte)(newbytes[1] | 0x80);
                            newbytes = newbytes.Concat(BitConverter.GetBytes(offset)).ToArray();
                        }

                        if (!ptr1 && !ptr2)
                        {
                            newbytes = new byte[] { 1, 0xC0 };
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg2] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                        }
                    }

                    Add(newbytes);
                    pos += newbytes.Length;
                    break;

                case "and":
                    if ((reg32.ContainsKey(reg1) && (reg2 == "")))
                    {
                        newbytes = new byte[] { 0x83, 0xE0 };
                        if ((Math.Abs(val2) < 0x80))
                        {
                            newbytes[0] = (byte)(newbytes[0] | 2);
                            newbytes = newbytes.Concat(new byte[] { (byte)(val2 & 0xFF) }).ToArray();
                        }
                        else
                        {
                            if ((reg1 == "eax"))
                            {
                                newbytes = new byte[] { 0x25 };
                            }
                            newbytes = newbytes.Concat(BitConverter.GetBytes(val2)).ToArray();
                        }
                        newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                    }

                    if (reg32.ContainsKey(reg1) && reg32.ContainsKey(reg2))
                    {
                        newbytes = new byte[] { 0x21, 0 };

                        if (ptr1)
                        {
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg2] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                        }
                        if (ptr2)
                        {
                            newbytes[0] = (byte)(newbytes[0] | 0x2);
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg1] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg2]);
                        }

                        if (!(ptr1 || ptr2))
                        {
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg2] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                            newbytes[1] = (byte)(newbytes[1] | 0xC0);
                        }

                        int offset;
                        offset = plus1 + plus2;

                        if ((Math.Abs(offset) < 0x80))
                        {
                            if ((offset > 0))
                            {
                                newbytes[1] = (byte)(newbytes[1] | 0x40);
                                newbytes = newbytes.Concat(new byte[] { (byte)(offset & 0xFF) }).ToArray();
                            }
                        }
                        if ((Math.Abs(offset) > 0x7F))
                        {
                            newbytes[1] = (byte)(newbytes[1] | 0x80);
                            newbytes = newbytes.Concat(BitConverter.GetBytes(offset)).ToArray();
                        }

                        if (!ptr1 && !ptr2)
                        {
                            newbytes = new byte[] { 0x21, 0xC0 };
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg2] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                        }
                    }

                    Add(newbytes);
                    pos += newbytes.Length;
                    break;

                case "call":
                    if (!ptr1)
                    {
                        if (reg32.ContainsKey(reg1))
                        {
                            newbytes = new byte[] { 0xFF, 0xD0 };
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                        }
                        else
                        {
                            newbytes = new byte[] { 0xE8 };
                            int _addr = Convert.ToInt32(val1) - pos - 5;
                            newbytes = newbytes.Concat(BitConverter.GetBytes(_addr)).ToArray();
                        }
                    }
                    else
                    {
                        if ((Math.Abs(plus1) < 0x80))
                        {
                            if ((plus1 > 0))
                            {
                                newbytes = new byte[] { 0xFF, 0x10 };
                                newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                            }
                            else
                            {
                                newbytes = new byte[] { 0xFF, 0x50, 0 };
                                newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                                newbytes[2] = (byte)plus1;
                            }
                        }
                        else
                        {
                            newbytes = new byte[] { 0xFF, 0x90 };
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                            newbytes = newbytes.Concat(BitConverter.GetBytes(plus1)).ToArray();
                        }
                    }

                    Add(newbytes);
                    pos += newbytes.Length;
                    break;

                case "cmp":
                    if ((reg32.ContainsKey(reg1) && (reg2 == "")))
                    {
                        newbytes = new byte[] { 0x81, 0xF8 };
                        if ((Math.Abs(val2) < 0x80))
                        {
                            newbytes[0] = (byte)(newbytes[0] | 2);
                            newbytes = newbytes.Concat(new byte[] { (byte)(val2 & 0xFF) }).ToArray();
                        }
                        else
                        {
                            if ((reg1 == "eax"))
                            {
                                newbytes = new byte[] { 0x3D };
                            }
                            newbytes = newbytes.Concat(BitConverter.GetBytes(val2)).ToArray();
                        }
                        newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                    }

                    if (reg32.ContainsKey(reg1) && reg32.ContainsKey(reg2))
                    {
                        newbytes = new byte[] { 0x39, 0 };

                        if (ptr1)
                        {
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg2] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                        }
                        if (ptr2)
                        {
                            newbytes[0] = (byte)(newbytes[0] | 0x2);
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg1] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg2]);
                        }

                        if (!(ptr1 || ptr2))
                        {
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg2] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                            newbytes[1] = (byte)(newbytes[1] | 0xC0);
                        }

                        int offset;
                        offset = plus1 + plus2;

                        if ((Math.Abs(offset) < 0x80))
                        {
                            if ((offset > 0))
                            {
                                newbytes[1] = (byte)(newbytes[1] | 0x40);
                                newbytes = newbytes.Concat(new byte[] { (byte)(offset & 0xFF) }).ToArray();
                            }
                        }
                        if ((Math.Abs(offset) > 0x7F))
                        {
                            newbytes[1] = (byte)(newbytes[1] | 0x80);
                            newbytes = newbytes.Concat(BitConverter.GetBytes(offset)).ToArray();
                        }

                        if (!ptr1 && !ptr2)
                        {
                            newbytes = new byte[] { 0x39, 0xC0 };
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg2] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                        }
                    }

                    Add(newbytes);
                    pos += newbytes.Length;
                    break;

                case "je":
                    newbytes = new byte[] { 0xF, 0x84 };
                    addr = Convert.ToInt32(val1) - pos - 6;
                    newbytes = newbytes.Concat(BitConverter.GetBytes(addr)).ToArray();
                    Add(newbytes);
                    pos += newbytes.Length;
                    break;

                case "jmp":
                    if (!ptr1)
                    {
                        if (reg32.ContainsKey(reg1))
                        {
                            newbytes = new byte[] { 0xFF, 0xE0 };
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                        }
                        else
                        {
                            newbytes = new byte[] { 0xE9 };
                            int _addr = Convert.ToInt32(val1) - pos - 5;
                            newbytes = newbytes.Concat(BitConverter.GetBytes(_addr)).ToArray();
                        }
                    }
                    else
                    {
                        if ((Math.Abs(plus1) < 0x80))
                        {
                            if ((plus1 > 0))
                            {
                                newbytes = new byte[] { 0xFF, 0x20 };
                                newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                            }
                            else
                            {
                                newbytes = new byte[] { 0xFF, 0x60, 0 };
                                newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                                newbytes[2] = (byte)(plus1 | 0xFF);
                            }
                        }
                        else
                        {
                            newbytes = new byte[] { 0xFF, 0xA0 };
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                            newbytes = newbytes.Concat(BitConverter.GetBytes(plus1)).ToArray();
                        }
                    }

                    Add(newbytes);
                    pos += newbytes.Length;
                    break;

                case "jne":
                    newbytes = new byte[] { 0xF, 0x85 };
                    addr = Convert.ToInt32(val1) - pos - 6;
                    newbytes = newbytes.Concat(BitConverter.GetBytes(addr)).ToArray();
                    Add(newbytes);
                    pos += newbytes.Length;
                    break;

                case "mov":

                    //'TODO:  Complete
                    //If reg8.Contains(reg1) And reg8.Contains(reg2) Then
                    //    newbytes = { &H88, &HC0 }
                    //    newbytes(1) = newbytes(1) Or reg8(reg1)
                    //    newbytes(1) = newbytes(1) Or reg8(reg2) *8
                    //    'TODO:  Complete
                    //End If

                    if ((reg32.ContainsKey(reg1) && (reg2 == "")))
                    {
                        newbytes = new byte[] { 0xB8 };
                        newbytes[0] = (byte)(newbytes[0] | (int)(reg32[reg1]));
                        newbytes = newbytes.Concat(BitConverter.GetBytes(val2)).ToArray();
                    }

                    if (reg32.ContainsKey(reg1) && reg32.ContainsKey(reg2))
                    {
                        newbytes = new byte[] { 0x89, 0 };

                        if (ptr1)
                        {
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg2] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                        }
                        if (ptr2)
                        {
                            newbytes[0] = (byte)(newbytes[0] | 0x2);
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg1] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg2]);
                        }

                        if (!(ptr1 || ptr2))
                        {
                            newbytes[1] = (byte)(newbytes[1] | ((int)reg32[reg2] * 8));
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                            newbytes[1] = (byte)(newbytes[1] | 0xC0);
                        }

                        int offset;
                        offset = plus1 + plus2;

                        if (ptr1 && (reg1 == "esp") || ptr2 && (reg2 == "esp"))
                        {
                            newbytes = newbytes.Concat(new byte[] { 0x24 }).ToArray();
                        }

                        if ((Math.Abs(offset) < 0x80))
                        {
                            if (offset > 0 || ptr1 && (reg1 == "esp") || ptr2 && (reg2 == "esp"))
                            {
                                newbytes[1] = (byte)(newbytes[1] | 0x40);
                                newbytes = newbytes.Concat(new byte[] { (byte)(offset & 0xFF) }).ToArray();
                            }
                        }
                        if ((Math.Abs(offset) > 0x7F))
                        {
                            newbytes[1] = (byte)(newbytes[1] | 0x80);
                            newbytes = newbytes.Concat(BitConverter.GetBytes(offset)).ToArray();
                        }
                    }

                    Add(newbytes);
                    pos += newbytes.Length;
                    break;

                case "push":
                    if (!ptr1)
                    {
                        if (reg32.ContainsKey(reg1))
                        {
                            newbytes = new byte[] { 0x50 };
                            newbytes[0] = (byte)(newbytes[0] | (int)reg32[reg1]);
                        }
                        else
                        {
                            if ((Math.Abs(val1) < 0x100))
                            {
                                newbytes = new byte[] { 0x6A, 0 };
                                newbytes[1] = (byte)(val1 | 0xFF);
                            }
                            else
                            {
                                newbytes = new byte[] { 0x68 };
                                newbytes = newbytes.Concat(BitConverter.GetBytes(val1)).ToArray();
                            }
                        }
                    }
                    else
                    {
                        if ((Math.Abs(plus1) < 0x80))
                        {
                            if ((plus1 > 0))
                            {
                                newbytes = new byte[] { 0xFF, 0x30 };
                            }
                            else
                            {
                                newbytes = new byte[] { 0xFF, 0x70, 0 };
                                newbytes[2] = (byte)(plus1 | 0xFF);
                            }
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                        }
                        else
                        {
                            newbytes = new byte[] { 0xFF, 0xB0 };
                            newbytes[1] = (byte)(newbytes[1] | (int)reg32[reg1]);
                            newbytes = newbytes.Concat(BitConverter.GetBytes(plus1)).ToArray();
                        }
                    }

                    Add(newbytes);
                    pos += newbytes.Length;
                    break;
                case "ret":
                    newbytes = new byte[] { 0xC2 };
                    if (Math.Abs(val1) > 0)
                    {
                        newbytes = newbytes.Concat(BitConverter.GetBytes(val1)).ToArray();
                    }
                    else
                    {
                        newbytes[0] = (byte)(newbytes[0] | 1);
                    }

                    Add(newbytes);
                    pos += newbytes.Length;
                    break;
            }
        }

        //public override string ToString()
        //{
        //    string tmpstr = "";

        //    foreach (byte byt in bytes) 
        //    {
        //        tmpstr = (tmpstr + ("0x"+ (byt.PadLeft(2, "0") + ", ")));
        //    }
        //}
    }
}
