using System;
using System.Configuration;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Xero.Api.Core.Model;
using System.Security.Cryptography.X509Certificates;

namespace SPTExtract
{
    public partial class frmSPTExract : Form
    {
        Xero.Api.Example.Applications.Private.Core _XeroAPI;
        private static readonly Xero.Api.Example.Applications.Private.Settings _ApplicationSettings = new Xero.Api.Example.Applications.Private.Settings();

        public frmSPTExract()
        {
            InitializeComponent();
        }

        private void btnGetInvoices_Click(object sender, EventArgs e)
        {
            // get invoices
            DateTime fromDate = dteFrom.Value;
            DateTime toDate = dteTo.Value;
            List<Guid> contact = new List<Guid>();
            contact.Add(new Guid(comboBoxCustomer.SelectedValue.ToString()));
            String where = @"Type==""ACCREC"" && Status==""SUBMITTED"" && Date >= DateTime("+ fromDate.Year.ToString() + ", " + fromDate.Month.ToString() + ", " + fromDate.Day.ToString() + ") && Date <= DateTime(" + toDate.Year.ToString() + ", " + toDate.Month.ToString() + ", " + toDate.Day.ToString() + ")";
            List<Invoice> invoices = new List<Invoice>();
            List<Invoice> invoicesPaged = new List<Invoice>();
            int page = 1;
            do
            {
                invoicesPaged = (List<Xero.Api.Core.Model.Invoice>)_XeroAPI.Invoices.Page(page).Where(where).ContactIds(contact).Find();
                invoices.AddRange(invoicesPaged);
                page++;
            } while (invoicesPaged.Count() > 0);

            if(invoices.Count==0)
            {
                MessageBox.Show("No invoices found", "SPTExtract", MessageBoxButtons.OK);
                return;
            }

            // loop through invoices
            int loopCount = 0;
            foreach (Invoice invoice in invoices)
            {
                // loop the line items
                int lineCount = 1;
                foreach (LineItem lineItem in invoice.LineItems)
                {
                    // y position
                    int yvar = 10 + (21 * (loopCount + 1));

                    TextBox textBoxInvNum = new TextBox();
                    textBoxInvNum.Name = "txtInvNum" + loopCount.ToString();
                    textBoxInvNum.Text = invoice.Number + " - Line " + lineCount.ToString();
                    textBoxInvNum.Enabled = false;
                    textBoxInvNum.BackColor = Color.White;
                    textBoxInvNum.Width = 100;
                    textBoxInvNum.Location = new Point(20, yvar);
                    panelResults.Controls.Add(textBoxInvNum);

                    TextBox textBoxJobNum = new TextBox();
                    textBoxJobNum.Name = "txtJobNum" + loopCount.ToString();
                    textBoxJobNum.Width = 100;
                    textBoxJobNum.Text = GetJobNumberFromDescription(lineItem.Description);
                    textBoxJobNum.Location = new Point(195, yvar);
                    panelResults.Controls.Add(textBoxJobNum);

                    TextBox textBoxQuantity = new TextBox();
                    textBoxQuantity.Name = "txtQuantity" + loopCount.ToString();
                    textBoxQuantity.Width = 100;
                    textBoxQuantity.Enabled = false;
                    textBoxQuantity.BackColor = Color.White;
                    textBoxQuantity.Text = String.Format("{0:N2}", lineItem.Quantity);
                    textBoxQuantity.Location = new Point(300, yvar);
                    panelResults.Controls.Add(textBoxQuantity);

                    TextBox textBoxUnitPrice = new TextBox();
                    textBoxUnitPrice.Name = "txtUnitPrice" + loopCount.ToString();
                    textBoxUnitPrice.Width = 100;
                    textBoxUnitPrice.Enabled = false;
                    textBoxUnitPrice.BackColor = Color.White;
                    textBoxUnitPrice.Text = String.Format("{0:N2}", lineItem.UnitAmount);
                    textBoxUnitPrice.Location = new Point(405, yvar);
                    panelResults.Controls.Add(textBoxUnitPrice);

                    TextBox textBoxSupplierNum = new TextBox();
                    textBoxSupplierNum.Name = "txtSupplierNum" + loopCount.ToString();
                    textBoxSupplierNum.Width = 100;
                    textBoxSupplierNum.Text = txtSupplierNumber.Text;
                    textBoxSupplierNum.Location = new Point(510, yvar);
                    panelResults.Controls.Add(textBoxSupplierNum);

                    TextBox textBoxBuyerNum = new TextBox();
                    textBoxBuyerNum.Name = "txtBuyerNum" + loopCount.ToString();
                    textBoxBuyerNum.Width = 100;
                    textBoxBuyerNum.Text = txtBuyerNumber.Text;
                    textBoxBuyerNum.Location = new Point(615, yvar);
                    panelResults.Controls.Add(textBoxBuyerNum);

                    TextBox textBoxDueDate = new TextBox();
                    textBoxDueDate.Name = "txtDueDate" + loopCount.ToString();
                    textBoxDueDate.Width = 100;
                    textBoxDueDate.Text = dteDueDate.Value.ToString("dd/MM/yy");
                    textBoxDueDate.Location = new Point(720, yvar);
                    panelResults.Controls.Add(textBoxDueDate);

                    loopCount++;
                    lineCount++;
                }
            }
            txtRowCount.Text = loopCount.ToString();
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(SPTExtract.Properties.Settings.Default.folder))
            {
                MessageBox.Show("You must select a folder to save the CSV file to, click the Folder button", "SPTExtract", MessageBoxButtons.OK);
                return;
            }

            String path = SPTExtract.Properties.Settings.Default.folder + @"\SPTExtract_" + DateTime.Now.ToString("dd-MM-yyyy") + ".csv";
            StreamWriter file = new StreamWriter(path);
            
            file.WriteLine("jobnum,Qty,UnitPriceGBP,supplier,buyer,DueDate");

            for (int i = 0; i < Convert.ToInt32(txtRowCount.Text); i++)
            {                    
                String csvLine = "";
                TextBox textBoxJobNumberTemp = (TextBox)panelResults.Controls.Find("txtJobNum" + i.ToString(), false)[0];
                String jobNum = textBoxJobNumberTemp.Text;
                TextBox textQuantityTemp = (TextBox)panelResults.Controls.Find("txtQuantity" + i.ToString(), false)[0];
                String quantity = textQuantityTemp.Text.Replace(",", String.Empty);
                TextBox textBoxUnitPriceTemp = (TextBox)panelResults.Controls.Find("txtUnitPrice" + i.ToString(), false)[0];
                String unitPrice = textBoxUnitPriceTemp.Text.Replace(",", String.Empty);
                TextBox textSupplierNumTemp = (TextBox)panelResults.Controls.Find("txtSupplierNum" + i.ToString(), false)[0];
                String supplierNum = textSupplierNumTemp.Text;
                TextBox textBoxBuyerNumTemp = (TextBox)panelResults.Controls.Find("txtBuyerNum" + i.ToString(), false)[0];
                String buyerNum = textBoxBuyerNumTemp.Text;
                TextBox textBoxDueDateTemp = (TextBox)panelResults.Controls.Find("txtDueDate" + i.ToString(), false)[0];
                String dueDate = textBoxDueDateTemp.Text;

                csvLine = jobNum + "," + quantity + "," + unitPrice + "," + supplierNum + "," + buyerNum + "," + dueDate;
                file.WriteLine(csvLine);
            }
            file.Close();
            MessageBox.Show("Extract complete. File extracted to:" + Environment.NewLine + path, "SPTExtract", MessageBoxButtons.OK);
        }

        private String GetJobNumberFromDescription(String description)
        {
            String jobNum = "";
            try
            {
                if (!String.IsNullOrEmpty(description))
                {
                    int dashPos = description.LastIndexOf('-');
                    if (dashPos > -1)
                    {
                        jobNum = description.Substring(dashPos, description.Length - dashPos);
                        jobNum = jobNum.Replace('-', ' ');
                        jobNum = jobNum.Trim(' ');
                    }
                }
            }
            catch (Exception)
            {
                return jobNum;
            }

            return jobNum;
        }

        private void toolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            String orgName = _XeroAPI.Organisation.Name;

            String message = "SPTExtract is a simple tool to get invoices from Xero and extract them to a csv file so that this can then be shared with a customer."
            + Environment.NewLine
            + Environment.NewLine
            + "SPTExtract is currently connected to the Xero organisation named:"
            + Environment.NewLine
            + orgName
            + Environment.NewLine
            + Environment.NewLine
            + "If you have any issues using SPTExtract contact John Leach."
            ;

            MessageBox.Show(message, "SPTExtract", MessageBoxButtons.OK);
        }

        private void toolStripMenuItemXero_Click(object sender, EventArgs e)
        {
            String shortCode = _XeroAPI.Organisation.ShortCode;

            String targetURL = @"https://go.xero.com/organisationlogin/default.aspx?shortcode="+ shortCode + "&redirecturl=/Accounts/Receivable/Dashboard/";
            System.Diagnostics.Process.Start(targetURL);
        }

        private void frmSPTExract_Load(object sender, EventArgs e)
        {
            txtRowCount.Visible = false;
            this.BackgroundImage = Properties.Resources.spt_background;

            DateTime tomorrow = DateTime.Now.AddDays(1);
            dteDueDate.Value = tomorrow;

            txtSupplierNumber.Text = SPTExtract.Properties.Settings.Default.supplierNumber;
            txtBuyerNumber.Text = SPTExtract.Properties.Settings.Default.buyerNumber;
            lblFolder.Text = SPTExtract.Properties.Settings.Default.folder;

            // inistalise the Xero API
            X509Certificate2 certificate = new X509Certificate2();
            certificate.Import(Properties.Resources.sptextract_privatekey, _ApplicationSettings.SigningCertificatePassword, X509KeyStorageFlags.MachineKeySet);
            _XeroAPI = new Xero.Api.Example.Applications.Private.Core(certificate, false)
            {
                UserAgent = "SPTExtract",
            };

            // load customers
            String where = @"IsCustomer=true";
            List<Contact> contacts = new List<Contact>();
            contacts = (List<Contact>)_XeroAPI.Contacts.Where(where).Find();
            comboBoxCustomer.DisplayMember = "Name"; // Column Name
            comboBoxCustomer.ValueMember = "Id";  // Column Name
            comboBoxCustomer.DataSource = contacts;
            comboBoxCustomer.SelectedValue = new Guid(SPTExtract.Properties.Settings.Default.contactId);

        }

        private void frmSPTExract_FormClosed(object sender, FormClosedEventArgs e)
        {
            SPTExtract.Properties.Settings.Default.supplierNumber = txtSupplierNumber.Text;
            SPTExtract.Properties.Settings.Default.buyerNumber = txtBuyerNumber.Text;
            SPTExtract.Properties.Settings.Default.contactId = comboBoxCustomer.SelectedValue.ToString();
            SPTExtract.Properties.Settings.Default.Save();
        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    SPTExtract.Properties.Settings.Default.folder = fbd.SelectedPath;
                    lblFolder.Text = fbd.SelectedPath;
                    SPTExtract.Properties.Settings.Default.Save();
                }
            }
        }
    }

}
