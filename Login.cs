using ATMSimulator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATMSimulator
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();            
        }

        DataClasses1DataContext db = new DataClasses1DataContext();

        private void Login_Load(object sender, EventArgs e)
        {

        }

        int attempts = 1;
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() == "" || textBox2.Text.Trim() == "") 
            {
                MessageBox.Show("The field cannot be empty! ");
            }else
            {
                // Login Query
                var user = (from s in db.Customers where s.Name == textBox1.Text.Trim() && s.CustomerID == textBox2.Text.Trim() select s);
                if (user.Count() > 0)
                {
                    foreach (var item in user)
                    {
                        if (item.Name == "Sysadmin")
                        {
                            this.Hide();
                            AdminPage frmadmin = new AdminPage();
                            frmadmin.Name = textBox1.Text;
                            frmadmin.CustomerID = textBox2.Text;  // CustomerID
                            frmadmin.ShowDialog();
                        }
                        else
                        {
                            this.Hide();
                            UserPage frmuser = new UserPage();
                            frmuser.CustomerID = textBox2.Text;
                            frmuser.ShowDialog();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Credential, try again! " + attempts + " out of 3");
                    attempts++;
                }

                if (attempts == 4)
                {
                    MessageBox.Show("You've reached your maximum number of attempts... \n Program will now close ");
                    Application.Exit();
                }
            }         

        }

        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
