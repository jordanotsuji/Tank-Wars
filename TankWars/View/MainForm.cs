/// Written by Kyle Charlton and Jordan Otsuji
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Controller;
using Model;

namespace View
{
    /// <summary>
    /// A class to represent the main form of the application.
    /// </summary>
    public partial class MainForm : Form
    {
        // The model to use for TankWars
        GameController controller;
        World theWorld;

        // Variables to handle a framerate of 60fps
        const int FrameTimeMS = 16;
        Stopwatch sinceLastRefresh;

        // UI Elements
        DrawingPanel drawingPanel;
        Button startButton;
        Button helpButton;
        Label nameLabel;
        TextBox nameText;
        Label serverLabel;
        TextBox serverText;

        /// <summary>
        /// Creates a new main form.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // Set the window size
            ClientSize = new Size(Constants.VIEW_SIZE_X, Constants.VIEW_SIZE_Y + Constants.MENU_SIZE);

            // Place and add the button
            startButton = new Button();
            startButton.Location = new Point(260, 7);
            startButton.Size = new Size(70, 20);
            startButton.Text = "Start";
            startButton.TabStop = false;
            this.Controls.Add(startButton);
            startButton.Click += new EventHandler(this.connectButton_Click);

            // Place and add the name label
            nameLabel = new Label();
            nameLabel.Text = "Name:";
            nameLabel.Location = new Point(5, 10);
            nameLabel.Size = new Size(40, 15);
            nameLabel.TabStop = false;
            this.Controls.Add(nameLabel);

            // Place and add the name textbox
            nameText = new TextBox();
            nameText.Text = "player";
            nameText.Location = new Point(46, 7);
            nameText.Size = new Size(70, 15);
            nameText.TabStop = false;
            this.Controls.Add(nameText);

            // Place and add the name label
            serverLabel = new Label();
            serverLabel.Text = "Server:";
            serverLabel.Location = new Point(130, 10);
            serverLabel.Size = new Size(45, 15);
            startButton.TabStop = false;
            this.Controls.Add(serverLabel);

            // Place and add the name textbox
            serverText = new TextBox();
            serverText.Text = "localhost";
            serverText.Location = new Point(175, 7);
            serverText.Size = new Size(70, 15);
            serverText.TabStop = false;
            this.Controls.Add(serverText);

            // Place and add the help button
            helpButton = new Button();
            helpButton.Location = new Point(Constants.VIEW_SIZE_X - 55, 7);
            helpButton.Size = new Size(50, 20);
            helpButton.Text = "Help";
            helpButton.TabStop = false;
            this.Controls.Add(helpButton);
            helpButton.Click += helpButton_Click;

            // Add the drawing panel
            drawingPanel = new DrawingPanel();
            drawingPanel.Location = new Point(0, Constants.MENU_SIZE);
            drawingPanel.Size = new Size(Constants.VIEW_SIZE_X, Constants.VIEW_SIZE_Y);
            drawingPanel.TabStop = false;
            drawingPanel.BackColor = Color.Black;

            this.Controls.Add(drawingPanel);

            this.Invalidate();
            sinceLastRefresh = new Stopwatch();
            sinceLastRefresh.Start();
        }

        /// <summary>
        /// Initializes the GameController
        /// </summary>
        private void InitializeGameController()
        {
            controller = new GameController();

            controller.UpdateArrived += OnReady;
            controller.TankDied += drawingPanel.CreateExplosion;

            // Add key handlers
            drawingPanel.MouseDown += new MouseEventHandler(controller.HandleMouseClickDown);
            drawingPanel.MouseUp += new MouseEventHandler(controller.HandleMouseClickUp);
            drawingPanel.MouseMove += new MouseEventHandler(controller.HandleMouseMovement);
            this.KeyUp += new KeyEventHandler(controller.HandleKeyRelease);
            this.KeyDown += new KeyEventHandler(controller.HandleKeyDown);
        }

        /// <summary>
        /// Connects to the server and starts a game of TankWars.
        /// </summary>
        private void connectButton_Click(object sender, EventArgs e)
        {
            startButton.Enabled = false;
            nameText.Enabled = false;
            serverText.Enabled = false;

            drawingPanel.Focus();
            this.KeyPreview = true;

            if (serverText.Text == "")
            {
                OnError("Server cannot be empty");
                return;
            }
            else if (nameText.Text.Length > 16)
            {
                OnError("Player name cannot exceed 16 characters");
                return;
            }
            else if (nameText.Text == "")
            {
                OnError("Player name cannot be empty");
                return;
            }

            InitializeGameController();
            controller.Start(serverText.Text, nameText.Text, OnError);
        }

        /// <summary>
        /// Creates a message box with helpful information.
        /// </summary>
        private void helpButton_Click(object sender, EventArgs e)
        {
            string msg = "W:\t\tMove up\n" +
                "A:\t\tMove left\n" +
                "S:\t\tMove down\n" +
                "D:\t\tMove right\n" +
                "Mouse:\t\tAim\n" +
                "Left Click:\tFire projectile\n" +
                "Right Click:\tFire beam\n";
            MessageBox.Show(this, msg);
        }

        /// <summary>
        /// An error handler delegate that displays a message to the user.
        /// </summary>
        /// <param name="msg">The message to display</param>
        private void OnError(string msg) => this.Invoke((Action)(() =>
        {
            MessageBox.Show(this, msg);

            this.KeyPreview = false;

            startButton.Enabled = true;
            nameText.Enabled = true;
            serverText.Enabled = true;

        }));

        /// <summary>
        /// A delagate to be called when the server is connected and ready.
        /// </summary>
        private void OnReady() => this.Invoke((Action)(() =>
        {
            // Place and add the drawing panel
            theWorld = controller.GetWorld();
            this.drawingPanel.SetWorld(theWorld);

            controller.UpdateArrived -= OnReady;
            controller.UpdateArrived += OnFrame;
        }));

        /// <summary>
        /// Handler for the controller's UpdateArrived event
        /// </summary>
        private void OnFrame()
        {
            if (sinceLastRefresh.ElapsedMilliseconds >= FrameTimeMS)
            {
                sinceLastRefresh.Restart();

                try
                {
                    this.Invoke((Action)(() => this.Invalidate(true)));
                }
                catch (Exception)
                {
                    // HACK: This catches an exception that triggers when
                    // closing the form and a frame update is attempted
                }
            }
        }
    }
}
