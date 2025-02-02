﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RemoteTestApplication
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Goodbye, World!";
            Console.WriteLine("Button1 clicked.");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblSystem.Text = Environment.OSVersion.ToString();
            lblOS.Text = Environment.OSVersion.Platform.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("What's your name?");
            button2.Text = "Your name is " + Console.ReadLine();
        }
    }
}
