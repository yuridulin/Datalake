using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcpConsole
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		static string BytesToString(byte[] bytes)
		{
			string s = "";
			if (bytes.Length > 0)
			{
				for (int i = 0; i < bytes.Length; i++) s += bytes[i].ToString("X2") + " ";
				s = s.Substring(0, s.Length - 1);
			}
			return s;
		}

		static byte[] StringToBytes(string s)
		{
			string[] raw = s.Split(' ');
			byte[] bytes = new byte[raw.Length];
			for (byte i = 0; i < raw.Length; i++)
			{
				bytes[i] = Convert.ToByte(raw[i], 16);
			}
			return bytes;
		}

		TcpClient Client { get; set; }

		NetworkStream Stream { get; set; }

		void GetSettings()
		{
			try
			{
				var lines = File.ReadAllLines(Application.StartupPath + "\\settings.txt");

				richTextBox1.Clear();

				foreach (var line in lines)
				{
					if (line.IndexOf('=') > -1)
					{
						string key = line.Substring(0, line.IndexOf('=')).Trim();
						string value = line.Substring(line.IndexOf('=') + 1).Trim();

						if (key == "TX")
						{
							richTextBox1.AppendText(value + "\n");
						}
						else if (key == "IP")
						{
							textBox1.Text = value;
						}
						else if (key == "PORT")
						{
							textBox2.Text = value;
						}
					}
				}

				MessageBox.Show("Настройки прочитаны", "Успех!", MessageBoxButtons.OK);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
			}
		}

		void WriteSettings()
		{
			try
			{
				var lines = new List<string>
				{
					"IP = " + textBox1.Text,
					"PORT = " + textBox2.Text
				};

				foreach (var line in richTextBox1.Lines)
				{
					if (line.Length > 0)
					{
						lines.Add("TX = " + line);
					}
				}

				File.WriteAllLines(Application.StartupPath + "\\settings.txt", lines.ToArray());

				MessageBox.Show("Настройки записаны", "Успех!", MessageBoxButtons.OK);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
			}
		}

		void SetupConn()
		{
			try
			{
				Client = new TcpClient();
				Client.Connect(textBox1.Text, int.Parse(textBox2.Text));

				Stream = Client.GetStream();

				label1.Text = "Соединено";
				Erase();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
				CloseConn();
			}
		}

		void CloseConn()
		{
			try
			{
				Client.Close();
			}
			catch { }

			label1.Text = "";
		}

		void Erase()
		{
			richTextBox2.Text = "";
		}

		void ByteExchange(string line)
		{
			try
			{
				int read = 0;

				byte[] tx = StringToBytes(line);
				richTextBox2.AppendText("TX: " + line + "\n");

				Stream.Write(tx, 0, tx.Length);

				byte[] rx = new byte[1024];
				Task.Run(() => { read = Stream.Read(rx, 0, rx.Length); }).Wait(TimeSpan.FromSeconds(2));

				if (read == 0)
				{
					richTextBox2.AppendText("RX: нет ответа\n");
				}
				else
				{
					rx = rx.Take(read).ToArray();

					richTextBox2.AppendText("RX: " + BytesToString(rx) + "\n");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
				CloseConn();
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			SetupConn();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			CloseConn();
		}

		private void button4_Click(object sender, EventArgs e)
		{
			string[] lines = richTextBox1.Lines.ToArray();
			
			foreach (var line in lines)
			{
				if (line.Length > 0)
				{
					ByteExchange(line);
					Task.Delay(100).Wait();
				}
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Erase();
		}

		private void button5_Click(object sender, EventArgs e)
		{
			GetSettings();
		}

		private void button6_Click(object sender, EventArgs e)
		{
			WriteSettings();
		}
	}
}
