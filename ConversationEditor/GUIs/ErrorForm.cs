using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Mail;

namespace ConversationEditor
{
    public partial class ErrorForm : Form
    {
        public ErrorForm()
        {
            InitializeComponent();
        }

        private Exception m_exception;

        public void SetException(Exception exception)
        {
            this.propertyGrid1.SelectedObject = exception;
            m_exception = exception;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string from = "ConversationEditor1234@hotmail.com";
            string from = "conversationeditor12345@gmail.com";
            string to = "thatguyiknow5@hotmail.com";
            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
            mail.To.Add(to);
            mail.From = new MailAddress(from, "Conversation Editor", System.Text.Encoding.UTF8);
            mail.Subject = "Conversation Editor error report";
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = false;
            mail.Priority = MailPriority.Normal;

            StringBuilder message = new StringBuilder();

            for (Exception exception = m_exception; exception != null; exception = exception.InnerException)
            {
                message.Append("Error report from: " + DateTime.Now); message.Append(Environment.NewLine);
                message.Append("Exception type: " + exception.GetType()); message.Append(Environment.NewLine);
                message.Append("Exception Message: " + exception.Message); message.Append(Environment.NewLine);
                message.Append("StackTrace: " + exception.StackTrace); message.Append(Environment.NewLine);
                if (exception.InnerException != null)
                    message.Append("Due To: "); message.Append(Environment.NewLine);
            }
            mail.Body = message.ToString();

            SmtpClient client = new SmtpClient();
            //client.Credentials = new System.Net.NetworkCredential(from, "shane5");
            client.Credentials = new System.Net.NetworkCredential(from, "a81d24b2-84ef-4191-a110-c9276df1f9ab");
            //client.Port = 25;
            client.Port = 587;
            //client.Host = "smtp.live.com";
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                client.Send(mail);
            }
            catch
            {
                MessageBox.Show("Failed to send mail");
            }

            Close();
        }
    }
}
