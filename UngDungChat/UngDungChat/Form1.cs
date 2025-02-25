﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace UngDungChat
{
    public partial class FormServer : Form
    {
        private Socket server, client;
        byte[] buff = new byte[1024];
        byte[] buff2 = new byte[1024];
        private delegate void updateUI(string massage);
        private updateUI updateUi;
        public FormServer()
        {
            InitializeComponent();
            updateUi = new updateUI(update);
            CheckForIllegalCrossThreadCalls = false;
        }
        bool isThoat = true;
        private void update(string m)
        {
            lbHienThi.Items.Add(m);
        }
        private void startServer()
        {
            EndPoint ep = new IPEndPoint(IPAddress.Any, 9050);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(ep);
            server.Listen(10);
            server.BeginAccept(new AsyncCallback(beginAccept), server);
            updateUi("Đang lắng nghe các kết nối....");
        }
        private void beginAccept(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            client = s.EndAccept(ar);
            updateUi("Đã nhận kết nối từ client " + client.RemoteEndPoint.ToString());
            string wc = "Xin chao client!...";
            buff2 = Encoding.ASCII.GetBytes(wc);
            client.BeginSend(buff2, 0, buff2.Length, SocketFlags.None, new AsyncCallback(sendata), client);
            client.BeginReceive(buff, 0, buff.Length, SocketFlags.None, new AsyncCallback(beginReceive), client);
        }
        private void beginReceive(IAsyncResult ia)
        {
            Socket s = (Socket)ia.AsyncState;
            int recv = 0;
            recv = s.EndReceive(ia);
            string recvei = Encoding.ASCII.GetString(buff, 0, recv);
            updateUi("Client: " + recvei);
            if (recvei.ToUpper() == "")
            {
                updateUi("Ngắt kết nối với client ");
                server.Close();
                client.Close();
                this.Close();
            }
            buff = new byte[1024];
            client.BeginReceive(buff, 0, buff.Length, SocketFlags.None, new AsyncCallback(beginReceive), client);
        }
        private void sendata(IAsyncResult ia)
        {
            client.EndSend(ia);
        }

        private void btnStart_Click_1(object sender, EventArgs e)
        {
            startServer();
            txtNhap.Focus();
        }
        private void send()
        {
            string hl = txtNhap.Text;
            byte[] gui = new byte[1024];
            gui = Encoding.ASCII.GetBytes(hl);
            txtNhap.Clear();
            updateUi("Server: " + hl);
            client.BeginSend(gui, 0, gui.Length, SocketFlags.None, new AsyncCallback(sendata), client);
        }

        private void btnGui_Click_1(object sender, EventArgs e)
        {
            send();
            txtNhap.Focus();
        }

        private void btnStop_Click_1(object sender, EventArgs e)
        {
            updateUi("Ngắt kết nối với client");
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            server.Close();
        }

        private void Server_Load_1(object sender, EventArgs e)
        {
           //this.IsMdiContainer = false;
            
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            isThoat = false;
            this.Close();
            FormDangNhap f = new FormDangNhap();
            f.Show();

        }

        private void FormServer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isThoat)
            {
                Application.Exit();
            }
        }

        private void FormServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(isThoat)
            {
                if (MessageBox.Show("Bạn muốn thoát chương trình", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    e.Cancel = true;
            }
        }

        private void txtNhap_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                send();

            }
        }
    }
}
