using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Controls;

namespace SerialInterfaceClock
{
    public partial class ComboBoxSerial
    {

        /// <summary>
        /// public class SerialPortItem
        /// </summary>
        public class SerialportItem
        {
            public string Name;
            public string Value;

            /// <summary>
            /// Set Item name and value
            /// </summary>
            /// <param name="name"></param>
            /// <param name="value"></param>
            public SerialportItem(string name, string value)
            {
                Name = name; Value = value;
            }

            /// <summary>
            /// Returns Item name
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return Name;
            }
        }


        public void GetPortsAddToComboBox(ComboBox CbPorts)
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
            {
                List<SerialportItem> ListSerialPorts = new List<SerialportItem>();

                /* Get portnames and add them to the string array */
                string[] PortNames = SerialPort.GetPortNames();

                /* Get serialports from system, get portname and device name and add them to tList */
                var Ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var tList = (from n in PortNames join p in Ports on n equals p["DeviceID"].ToString() select n).ToList();

                if (PortNames != null || PortNames.Length != 0)
                {
                    List<string> portsOrdered = new List<string>();

                    /* Sort all items in list on alphabetical order */
                    Array.Sort<string>(PortNames);          // Call sort.

                    foreach (var c in PortNames)
                    {
                        portsOrdered.Add(c);
                    }

                    tList.Sort();

                    /* Run thru list and add only the items that contains a device name to ListSerialPorts */
                    for (int x = 0; x < portsOrdered.Count(); x++)
                    {
                        for (int i = 0; i < tList.Count; i++)
                        {
                            if (tList[i].Contains(portsOrdered[x]))
                            {
                                ListSerialPorts.Add(new SerialportItem(tList[i], portsOrdered[x]));
                                portsOrdered.RemoveAt(x);
                            }
                        }
                    }

                    /* Check if the list count is higher than zero */
                    if (portsOrdered.Count() > 0)
                    {
                        /* Run thru the list and add the port with new name to ListSerialPorts */
                        for (int x = 0; x < portsOrdered.Count(); x++)
                        {
                            ListSerialPorts.Add(new SerialportItem((portsOrdered[x] + " - USB Serial Port"), portsOrdered[x]));
                        }
                    }
                }

                /* Clear combobox */
                CbPorts.Items.Clear();
                CbPorts.Text = string.Empty;

                /* Add serialports to combobox */
                if (ListSerialPorts.Count > 0)
                {
                    for (int index = 0; index < ListSerialPorts.Count; index++)
                        CbPorts.Items.Add(ListSerialPorts[index]);
                }
            }
        }
    }
}
