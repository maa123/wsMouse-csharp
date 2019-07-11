using System;

using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Net;

using WebSocketSharp;
using WebSocketSharp.Server;

using ZXing;


class TrackPad : WebSocketBehavior {
	[DllImport("user32.dll")]
	private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

	private async static Task<string> moveMouse(double x, double y) {
		mouse_event(0x01, (int)(-x/4.0), (int)(-y/4.0), 0, 0);
		await Task.Delay(1);
		mouse_event(0x01, (int)(-x/4.0), (int)(-y/4.0), 0, 0);
		return "";
	}
	private static void movesMouse(double x, double y) {
		mouse_event(0x01, (int)(-x/1.2), (int)(-y/1.2), 0, 0);
	}
	private static void scrolly(int sc) {
		mouse_event(0x800, 0, 0, sc, 0);
	}
	protected override void OnMessage (MessageEventArgs e) {
		string str = e.Data;
		if(string.Equals("cd", str)){
			mouse_event(0x02, 0, 0, 0, 0);
		}else if(string.Equals("cu", str)){
			mouse_event(0x04, 0, 0, 0, 0);
		}else{
			string[] strs = str.Split(',');
			if(string.Equals("p", strs[0])){
				double px = double.Parse(strs[1]);
				double py = double.Parse(strs[2]);
				if(px+py > 3){
					moveMouse(px, py);
				}else{
					movesMouse(px, py);
				}
			}else if(string.Equals("m", strs[0])){
				scrolly(-(int)double.Parse(strs[2]));
			}
		}
	}
}

class wsmouse{
	public static string GetLocalIPAddress() {
		IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList) {
			if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				return ip.ToString();
			}
		}
		return "127.0.0.1";
	}
	public static void Main(){
		Form mainfm = new Form();
		mainfm.Width = 300;
		mainfm.Height = 200;
		mainfm.Text = "TrackPad Bridge";
		PictureBox pb = new PictureBox();
		BarcodeWriter qrcode = new BarcodeWriter {Format = BarcodeFormat.QR_CODE, Options = new ZXing.QrCode.QrCodeEncodingOptions {
			ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.M,
			CharacterSet = "ISO-8859-1",
			Height = 130,
			Width = 130,
			Margin = 3
		}};
		pb.Size = new Size(150, 150);
		pb.SizeMode = PictureBoxSizeMode.CenterImage;
		pb.Image = qrcode.Write("ws://"+GetLocalIPAddress()+":8765");
		mainfm.Controls.Add(pb);
		var wssv = new WebSocketServer (8765);
		wssv.AddWebSocketService<TrackPad> ("/");
		wssv.Start ();
		Application.Run(mainfm);
		wssv.Stop ();
	}
}