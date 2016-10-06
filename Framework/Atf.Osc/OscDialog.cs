//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Applications.NetworkTargetServices;

namespace Sce.Atf.Applications
{
    internal partial class OscDialog : Form
    {
        public OscDialog(OscService oscService, OscCommandReceiver commandReceiver)
        {
            InitializeComponent();
            SuspendLayout();

            m_oscService = oscService;
            m_commandReceiver = commandReceiver;

            string myHostName;
            try
            {
                myHostName = Dns.GetHostName();
            }
            catch (SocketException)
            {
                myHostName = "<not available>";
            }
            m_hostName.Text = myHostName;

            int localIPAddressIndex = 0;
            int selectedIndex = 0;
            foreach (IPAddress receivingIPAddress in OscService.GetLocalIPAddresses())
            {
                m_receivingIPAddresses.Items.Add(receivingIPAddress.ToString());
                if (receivingIPAddress.Equals(m_oscService.ReceivingIPAddress))
                    selectedIndex = localIPAddressIndex;
                localIPAddressIndex++;
            }
            m_receivingIPAddresses.SelectedIndex = selectedIndex;

            m_receivingPortNumber.Text = m_oscService.ReceivingPort.ToString(CultureInfo.InvariantCulture);
            m_statusTextBox.Text = m_oscService.StatusMessage;

            m_destinationIPAddress.Text = m_oscService.DestinationEndpoint.Address.ToString();
            m_destinationPortNumber.Text = m_oscService.DestinationEndpoint.Port.ToString(CultureInfo.InvariantCulture);

            var addressInfos = new List<OscService.OscAddressInfo>(m_oscService.AddressInfos);
            addressInfos.Sort((info1, info2) => info1.Address.CompareTo(info2.Address));

            ListView.ListViewItemCollection items = m_listView.Items;
            foreach (OscService.OscAddressInfo info in addressInfos)
            {
                var item = new ListViewItem(new string[] {
                    info.Address,
                    info.PropertyName,
                    PropertyTypeToReadableString(info.PropertyType),
                    info.CompatibleType.ToString()
                });
                items.Add(item);
            }

            // Add commands
            if (m_commandReceiver != null)
            {
                foreach (string address in m_commandReceiver.GetOscAddresses())
                {
                    var item = new ListViewItem(new string[]
                    {
                        address,
                        "n/a",
                        "n/a",
                        "n/a"
                    });
                    items.Add(item);
                }
            }

            ResumeLayout();
        }

        private void toClipboardButton_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Open Sound Control (OSC) configuration information");
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine("This computer's host name: " + m_hostName.Text);
            sb.AppendLine("This app's local IP address: " + m_oscService.ReceivingIPAddress);
            sb.AppendLine("This app's port # for receiving OSC messages: " + m_oscService.ReceivingPort);
            sb.AppendLine();
            sb.AppendLine("Destination IP address: " + m_destinationIPAddress.Text);
            sb.AppendLine("Destination port #: " + m_destinationPortNumber.Text);
            sb.AppendLine();
            sb.AppendLine("Status:");
            sb.AppendLine(m_oscService.StatusMessage);
            sb.AppendLine();
            sb.AppendLine("OSC Address\tproperty name\tproperty type\tC# class name");

            foreach (OscService.OscAddressInfo info in m_oscService.AddressInfos)
            {
                sb.AppendLine(
                    info.Address + '\t' +
                    info.PropertyName + '\t' +
                    PropertyTypeToReadableString(info.PropertyType) + '\t' +
                    info.CompatibleType.ToString());
            }

            foreach (string address in m_commandReceiver.GetOscAddresses())
            {
                sb.AppendLine(
                    address + '\t' +
                    "n/a\t" +
                    "n/a\t" +
                    "n/a\t");
            }

            Clipboard.SetText(sb.ToString());
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            // Do the validation.
            IPEndPoint receivingEndPoint =
                TcpIpTargetInfo.TryParseIPEndPoint(m_receivingIPAddresses.Items[m_receivingIPAddresses.SelectedIndex].ToString() +
                ':' + m_receivingPortNumber.Text);
            if (receivingEndPoint == null)
            {
                MessageBox.Show("The receiving port number or IP address are not correctly formatted.".Localize());
                DialogResult = DialogResult.None;
                return;
            }

            IPEndPoint destinationEndPoint =
                TcpIpTargetInfo.TryParseIPEndPoint(m_destinationIPAddress.Text + ':' + m_destinationPortNumber.Text);
            if (destinationEndPoint == null)
            {
                MessageBox.Show("The destination port number or IP address are not correctly formatted.".Localize());
                DialogResult = DialogResult.None;
                return;
            }

            m_oscService.DestinationEndpoint = destinationEndPoint;
            m_oscService.ReceivingEndpoint = receivingEndPoint;
        }

        private string PropertyTypeToReadableString(Type propertyType)
        {
            if (propertyType == typeof(int))
                return "int";
            if (propertyType == typeof(float))
                return "float";
            if (propertyType == typeof(string))
                return "string";
            if (propertyType == typeof(bool))
                return "bool";
            return propertyType.ToString();
        }

        private void disableButton_Click(object sender, EventArgs e)
        {
            m_destinationIPAddress.Text = "255.255.255.255";
        }

        private readonly OscService m_oscService;
        private OscCommandReceiver m_commandReceiver;
    }
}
