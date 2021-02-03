using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KerberosConfigMgr
{
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 250,
                Height = 165,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label()
            {
                Left = 40,
                Top = 25,
                Text = text,
                Width = 150
            };
            TextBox textBox = new TextBox()
            {
                Left = 20,
                Top = 50,
                Width = 200
            };
            Button confirmation = new Button()
            {
                Text = "Ok",
                Left = 90,
                Width = 50,
                Top = 80,
                DialogResult = DialogResult.OK
            };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
