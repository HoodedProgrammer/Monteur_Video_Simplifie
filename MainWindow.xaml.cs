#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8625

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Monteur_Video_Simplifie_WFP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {            
            InitializeComponent();
            
            //Initializing the icons for each button related to the media element player
            init_button_icons();            

            //Initializing list of position buttons
            lsButton = new Button[] { pos1_bttn, pos2_bttn, pos3_bttn, pos4_bttn, pos5_bttn, pos6_bttn, pos7_bttn, pos8_bttn};

            // Assignation of the slider-related handlers
            slider_time.PreviewMouseDown += slider_Clicked;
            slider_time.PreviewMouseUp += slider_Unclicked;

            // Assignation or the timer-related handler
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            //Initializing the 'nb_max_chords_on_width' variable
            /*
            while (nb_max_chords_on_width == -1)
            {
                string result = Microsoft.VisualBasic.Interaction.InputBox("How many chords (maximum) do you want to inlay at the same time on the video width?", " ", "8");
                try
                {
                    int nb_result = int.Parse(result);
                    if (nb_result > 0 && nb_result < 9)
                        nb_max_chords_on_width = nb_result;
                    else
                        MessageBox.Show("Input can only be a number a number between 1 and 8 inclusive", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch
                    {MessageBox.Show("Input can only be a number a number between 1 and 8 inclusive", "Error", MessageBoxButton.OK, MessageBoxImage.Error);}
            }
            */

            //Initializing the 'offset' variable
            /*
            while (offset == -1)
            {
                string result = Microsoft.VisualBasic.Interaction.InputBox("What should be the space (in pixel) between 2 chords pictures when inlaying?", " ", "5");
                try
                {
                    int nb_result = int.Parse(result);
                    if (nb_result > 0 && nb_result < 101)
                        offset = nb_result;
                    else
                        MessageBox.Show("Input can only be a number a number between 1 and 100 inclusive", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch
                    {MessageBox.Show("Input can only be a number a number between 1 and 100 inclusive", "Error", MessageBoxButton.OK, MessageBoxImage.Error);}
            }            
            */
        }

        private void init_button_icons()
        {
            //Loading the pictures contained by the media player button            
            play_pause_bttn.Content = new Image { Source = new BitmapImage(new Uri(imgs_root_dir + "Play.png", UriKind.RelativeOrAbsolute)) };
            prev_frame_bttn.Content = new Image { Source = new BitmapImage(new Uri(imgs_root_dir + "Previous_frame.png", UriKind.RelativeOrAbsolute)) };
            prev_10_bttn.Content    = new Image { Source = new BitmapImage(new Uri(imgs_root_dir + "Previous_10s.png", UriKind.RelativeOrAbsolute)) };            
            stop_bttn.Content       = new Image { Source = new BitmapImage(new Uri(imgs_root_dir + "Stop.png", UriKind.RelativeOrAbsolute)) };
            next_10_bttn.Content    = new Image { Source = new BitmapImage(new Uri(imgs_root_dir + "Next_10s.png", UriKind.RelativeOrAbsolute)) };
            next_frame_bttn.Content = new Image { Source = new BitmapImage(new Uri(imgs_root_dir + "Next_frame.png", UriKind.RelativeOrAbsolute)) };
            
            this.Icon = new BitmapImage(new Uri(imgs_root_dir + "icone_top_left.ico", UriKind.Relative));
        }

        /***********************************************************/
        /// 
        /// 
        ///               TIMER RELATED FUNCTIONS
        /// 
        /// 
        /***********************************************************/
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Duration checkbpx management
            if (mediaElementPlayer.NaturalDuration.HasTimeSpan)
                textbox_timespan_right.Text = mediaElementPlayer.NaturalDuration.TimeSpan.ToString();

            // Current timespan checkbox management            
            if (mediaElementPlayer != null)
            {
                textbox_timespan_left.Text = mediaElementPlayer.Position.Hours.ToString("00") + ":" + mediaElementPlayer.Position.Minutes.ToString("00") + ":" + mediaElementPlayer.Position.Seconds.ToString("00");

                if (mediaElementPlayer.NaturalDuration.HasTimeSpan && isPlaying)
                {
                    slider_time.Value = mediaElementPlayer.Position.Divide(mediaElementPlayer.NaturalDuration.TimeSpan) * 100;
                    if (mediaElementPlayer.Position == mediaElementPlayer.NaturalDuration.TimeSpan)
                        stop_bttn_Click(stop_bttn, null);
                }
            }
        }

        /***********************************************************/
        /// 
        /// 
        ///               SLIDER RELATED FUNCTIONS
        /// 
        /// 
        /***********************************************************/
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (slider_time.IsMouseOver && isSliderMouseClicked && isPlaying)
                mediaElementPlayer.Pause();

            if (mediaElementPlayer.NaturalDuration.HasTimeSpan)
                mediaElementPlayer.Position = (mediaElementPlayer.NaturalDuration.TimeSpan * e.NewValue) / 100;

            if (isPlaying)
                mediaElementPlayer.Play();
        }

        private void slider_Clicked(object sender, MouseButtonEventArgs e)
        {
            isSliderMouseClicked = true;
        }

        private void slider_Unclicked(object sender, MouseButtonEventArgs e)
        {
            isSliderMouseClicked = false;
        }

        /***********************************************************/
        /// 
        /// 
        ///               VIDEO PLAYER MANAGEMENT
        /// 
        /// 
        /***********************************************************/
        private void video_drop(object sender, DragEventArgs e)
        {
            // Checking that what is being dropped is indeed a file
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {                
                string extension = System.IO.Path.GetExtension(((string[])e.Data.GetData(DataFormats.FileDrop))[0]).ToLower();

                if (extension == ".avi" || extension == ".mp4" || extension == ".wmv" || extension == ".mov")                
                    video_load(((string[])e.Data.GetData(DataFormats.FileDrop))[0]);                
                else
                    MessageBox.Show("Warning! The only accepted extensions are: '*.avi', '*.mp4', '*.wmv', '*.mov'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void load_video_from_bttn_Click(object sender, RoutedEventArgs e)
        {
            filepath_input_video_dialog.FileName = "";
            filepath_input_video_dialog.ShowDialog();
            if (filepath_input_video_dialog.FileName != "")
            {
                filepath_input_video_dialog.InitialDirectory = Path.GetDirectoryName(filepath_input_video_dialog.FileName);
                input_filepath_label.Content = filepath_input_video_dialog.FileName;
                video_load(filepath_input_video_dialog.FileName);
            }            
        }

        private void video_load (string filepath)
        {
            filepath_input_video = filepath;
            input_filepath_label.Content = filepath_input_video;

            // Loading the video in the mediaElementPlayer
            label_title.Content = System.IO.Path.GetFileName(filepath_input_video);
            mediaElementPlayer.Source = new Uri(filepath_input_video);
            mediaElementPlayer.Stretch = Stretch.Uniform;
            mediaElementPlayer.Play(); //The Play function is added to avoid a bug when the user imports a new video, that he clicks the +10s button, and then he press the Play button (The video starts from the beginning, ignoring the +10s button press)
            stop_bttn_Click(stop_bttn, null);

            //Emptying the modif_list and ImageInfo list to start fresh
            modif_info_list.Items.Clear();
            list_img_info.Clear();            
        }

        private void set_output_video_directory_bttn_Click(object sender, RoutedEventArgs e)
        {
            output_video_directory_dialog.FileName = "";
            output_video_directory_dialog.ShowDialog();

            if (output_video_directory_dialog.FileName != "")
            {
                filepath_output_directory = Path.GetDirectoryName(output_video_directory_dialog.FileName);
                if (!filepath_output_directory.EndsWith("\\")) filepath_output_directory+= "\\";
                filepath_output_filename = Path.GetFileName(output_video_directory_dialog.FileName);                
                output_directory_label.Content = filepath_output_directory + filepath_output_filename;
            }

        }

        private void play_pause_bttn_Click(object sender, RoutedEventArgs e)
        {
            if (!mediaElementPlayer.HasVideo)
                return;

            if (!isPlaying)
            {
                mediaElementPlayer.Play();
                isPlaying = true;
                play_pause_bttn.Content = new Image { Source = new BitmapImage(new Uri(imgs_root_dir + "Pause.png", UriKind.RelativeOrAbsolute)) };
            }
            else
            {
                mediaElementPlayer.Pause();
                isPlaying = false;
                play_pause_bttn.Content = new Image { Source = new BitmapImage(new Uri(imgs_root_dir + "Play.png", UriKind.RelativeOrAbsolute)) };
            }
        }

        private void stop_bttn_Click(object sender, RoutedEventArgs e)
        {
            mediaElementPlayer.Stop();
            isPlaying = false;
            play_pause_bttn.Content = new Image { Source = new BitmapImage(new Uri(imgs_root_dir + "Play.png", UriKind.RelativeOrAbsolute)) };
            slider_time.Value = 0;
        }
        private void prev_10_bttn_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElementPlayer != null && mediaElementPlayer.NaturalDuration.HasTimeSpan)
            {
                if (mediaElementPlayer.Position - TimeSpan.FromSeconds(10) > new TimeSpan(0, 0, 0))
                    mediaElementPlayer.Position -= TimeSpan.FromSeconds(10);
                else
                    mediaElementPlayer.Position = new TimeSpan(0, 0, 0);
                slider_time.Value = (mediaElementPlayer.Position * 100) / mediaElementPlayer.NaturalDuration.TimeSpan;
            }
        }
        private void next_10_bttn_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElementPlayer != null && mediaElementPlayer.NaturalDuration.HasTimeSpan)
            {
                if (mediaElementPlayer.Position + TimeSpan.FromSeconds(10) < mediaElementPlayer.NaturalDuration.TimeSpan)
                    mediaElementPlayer.Position += TimeSpan.FromSeconds(10);
                else
                    mediaElementPlayer.Position = mediaElementPlayer.NaturalDuration.TimeSpan;
                slider_time.Value = (mediaElementPlayer.Position * 100) / mediaElementPlayer.NaturalDuration.TimeSpan;
            }
        }

        private void prev_frame_bttn_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElementPlayer != null && mediaElementPlayer.NaturalDuration.HasTimeSpan)
            {
                if (mediaElementPlayer.Position - TimeSpan.FromMilliseconds(periode_frame) > new TimeSpan(0, 0, 0))
                    mediaElementPlayer.Position -= TimeSpan.FromMilliseconds(periode_frame);
                else
                    mediaElementPlayer.Position = new TimeSpan(0, 0, 0);
                slider_time.Value = (mediaElementPlayer.Position * 100) / mediaElementPlayer.NaturalDuration.TimeSpan;
            }
        }
        private void next_frame_bttn_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElementPlayer != null && mediaElementPlayer.NaturalDuration.HasTimeSpan)
            {
                if (mediaElementPlayer.Position + TimeSpan.FromMilliseconds(periode_frame) < mediaElementPlayer.NaturalDuration.TimeSpan)
                    mediaElementPlayer.Position += TimeSpan.FromMilliseconds(periode_frame);
                else
                    mediaElementPlayer.Position = mediaElementPlayer.NaturalDuration.TimeSpan;
                slider_time.Value = (mediaElementPlayer.Position * 100) / mediaElementPlayer.NaturalDuration.TimeSpan;
            }
        }

        /***********************************************************/
        /// 
        /// 
        ///               PICTURE INLAYING MANAGEMENT
        /// 
        /// 
        /***********************************************************/

        /* BLOCK 1 MANAGEMENT */
        private void picture_to_insert_DragOver(object sender, DragEventArgs e)
        {
            // Checking that what is being dropped is indeed a file
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                filepath_input_picture = "";
                string extension = System.IO.Path.GetExtension(((string[])e.Data.GetData(DataFormats.FileDrop))[0]).ToLower();

                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp")                
                    picture_to_inlay_import(((string[])e.Data.GetData(DataFormats.FileDrop))[0]);                                                       
                else
                    MessageBox.Show("Warning! The only accepted extensions are: '*.jpg', '*.jpeg', '*.png', '*.bmp'.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
        }

        private void load_picture_to_insert_from_bttn_Click (object sender, RoutedEventArgs e)
        {
            filepath_picture_to_insert_dialog.FileName = "";
            filepath_input_picture = "";

            filepath_picture_to_insert_dialog.ShowDialog();
            if (filepath_picture_to_insert_dialog.FileName != "")
            {
                filepath_picture_to_insert_dialog.InitialDirectory = Path.GetDirectoryName(filepath_picture_to_insert_dialog.FileName);
                picture_to_inlay_import(filepath_picture_to_insert_dialog.FileName);
            }
        }

        private void picture_to_inlay_import (string filepath)
        {
            filepath_input_picture = filepath;
            picture_to_insert.Fill = new ImageBrush() { ImageSource = new BitmapImage(new Uri(filepath_input_picture, UriKind.RelativeOrAbsolute))};

            if (mediaElementPlayer.HasVideo)
            {
                picture_to_insert_width_txtbox.Text = (Math.Floor((double)((mediaElementPlayer.NaturalVideoWidth / nb_max_chords_on_width) - offset))).ToString();                
                picture_to_insert_height_txtbox.Text = (autoscale_height_txtbox.IsChecked.GetValueOrDefault() ? "-1" : (pixel_width * 2).ToString());                
            }            
        }

        private void picture_to_insert_width_txtbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox width_txtbox = sender as TextBox;
            try
            {
                pixel_width = int.Parse(width_txtbox.Text);
            }
            catch
            {
                pixel_width = -1;
            }
        }

        private void picture_to_insert_height_txtbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox height_txtbox = sender as TextBox;
            try
            {
                pixel_height = int.Parse(height_txtbox.Text);
            }
            catch
            {
                pixel_height = -2;
            }
        }

        /* BLOCK 2 MANAGEMENT */
        private void autoscale_height_txtbox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            if (chkbox.IsChecked.GetValueOrDefault())
            {
                picture_to_insert_height_txtbox.IsReadOnly = true;

                if (picture_to_insert.Fill != null && mediaElementPlayer.HasVideo)
                    picture_to_insert_height_txtbox.Text = "-1";
                else
                    picture_to_insert_height_txtbox.Clear();
            }
            else
            {
                picture_to_insert_height_txtbox.IsReadOnly = false;
                if (picture_to_insert.Fill != null && mediaElementPlayer.HasVideo)
                    if (pixel_width != -1)
                        picture_to_insert_height_txtbox.Text = (pixel_width * 2).ToString();
                    else
                        picture_to_insert_height_txtbox.Text = "-1";
            }
        }

        private void pos_bttn_Click(object sender, RoutedEventArgs e)
        {
            Button receivedBttn = sender as Button;

            for (int i = 0; i < lsButton.Length; i++)
            {
                if (receivedBttn == lsButton[i])
                {                    
                    // Assignation of the 'selected_button' var, used to determine whether a position has been selected or not.
                    selected_button = i;                    

                    // Assignation of the 'position' var
                    position = (offset + ((offset + pixel_width) * selected_button)).ToString() + ":5";

                    // Font style management
                    lsButton[i].FontWeight = FontWeights.Bold;
                    lsButton[i].BorderThickness = new Thickness(3);

                    // Button Style management
                    lsButton[i].Background = new SolidColorBrush(Colors.Chartreuse);
                    lsButton[i].Foreground = new SolidColorBrush(Colors.BlueViolet);
                    lsButton[i].BorderBrush = new SolidColorBrush(Colors.BlueViolet);
                }
                else
                {
                    // Font style management
                    lsButton[i].FontWeight = FontWeights.Normal;
                    lsButton[i].BorderThickness = new Thickness(1);

                    // Button Style management
                    lsButton[i].Background = new SolidColorBrush(Colors.Black);
                    lsButton[i].Foreground = new SolidColorBrush(Colors.Chartreuse);
                    lsButton[i].BorderBrush = new SolidColorBrush(Colors.Chartreuse);
                }
            }
        }
        
        /* BLOCK 3 MANAGEMENT */
        private void capture_time_Click(object sender, RoutedEventArgs e)
        {
            Button receivedBttn = sender as Button;

            if (mediaElementPlayer.HasVideo)
            {
                if (receivedBttn == capture_start_time_bttn)
                    start_time_textbox.Text = mediaElementPlayer.Position.ToString();
                else if (receivedBttn == capture_stop_time_bttn)
                    stop_time_textbox.Text = mediaElementPlayer.Position.ToString();
            }
        }

        /* BLOCK 4 & HISTORY MANAGEMENT */
        private void add_picture_bttn_Click(object sender, RoutedEventArgs e)
        {

            if (!parametersAreOK())
                return;

            // Conversion of the start & stop time from string to float
            string[] split_start_time = start_time_textbox.Text.ToString().Split(":");
            string[] split_stop_time = stop_time_textbox.Text.ToString().Split(":");

            float start_time_seconds = int.Parse(split_start_time[0]) * 3600 + int.Parse(split_start_time[1]) * 60 + float.Parse(split_start_time[2].Replace(".", ","));
            float stop_time_seconds = int.Parse(split_stop_time[0]) * 3600 + int.Parse(split_stop_time[1]) * 60 + float.Parse(split_stop_time[2].Replace(".", ","));

            if (stop_time_seconds < start_time_seconds)
            {
                MessageBox.Show("The stop time can't occur before the start time.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            // Adding the modification to the modification list            
            ImagesInformation ii = new ImagesInformation()
            {
                Filepath = filepath_input_picture,
                Position = position,
                PixelWidth = pixel_width.ToString(),
                PixelHeight = pixel_height.ToString(),
                StartTime = start_time_seconds.ToString().Replace(",", "."),
                StopTime = stop_time_seconds.ToString().Replace(",", ".")
            };
            list_img_info.Add(ii);

            // Adding the information in the modif_info_list for the user to see them
            modif_info_list.Items.Add((modif_info_list.Items.Count+1).ToString() + " - " + "Picture '" + System.IO.Path.GetFileName(ii.Filepath) + "' with dimensions (" + ii.PixelWidth + "x" + ii.PixelHeight + ") will be added at the position '" + (selected_button + 1).ToString() + "': [" + ii.Position + "] between (" + start_time_textbox.Text + ") and (" + stop_time_textbox.Text + ").");

            // Resetting the interface for it to be clean again for the next picture add
            picture_to_insert.Fill = null;
            filepath_input_picture = "";
            pixel_width = -1;
            picture_to_insert_width_txtbox.Clear();
            pixel_height = -1;
            picture_to_insert_height_txtbox.Clear();
            pos_bttn_Click(null, null);
            selected_button = -1;
            start_time_textbox.Text = "00:00:00";
            stop_time_textbox.Text = "00:00:00";
        }

        private bool parametersAreOK()
        {
            bool hasError = false;

            // Making sure all the parameters are present for the picture to be added correctly 
            if (picture_to_insert.Fill == null || filepath_input_picture == "")
                MessageBox.Show("An error occured because there is no image to insert.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (pixel_width == -1 || pixel_height == -2)
                MessageBox.Show("An error occured because the picture width or height is incorrect.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (selected_button == -1)
                MessageBox.Show("An error occured because no position button is selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else if ((string)start_time_textbox.Text == "00:00:00")
                MessageBox.Show("An error occured because no start time is defined.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else if ((string)stop_time_textbox.Text == "00:00:00")
                MessageBox.Show("An error occured because no stop time is defined.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                hasError = true;
            return hasError;
        }

        /* EXPORT MANAGEMENT */
        private void export_func_bttn_Click(object sender, RoutedEventArgs e)
        {
            if (list_img_info.Count == 0)
            {
                MessageBox.Show("No modifications are to be applied.", "An error occured when trying to export.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Are you sure you want to apply those changes?", "Confirm save", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                string output_path = filepath_output_directory + filepath_output_filename;
                if (filepath_input_video == "" || filepath_output_directory == "")
                {
                    MessageBox.Show("An error occured when exporting the modifications because either there is no input file, or there is not output directory set.", "An error occured when trying to export.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }                
                if (Path.GetDirectoryName(filepath_input_video) == Path.GetDirectoryName(output_path))
                {
                    MessageBox.Show("Input folder can't be the same than output folder.", "An error occured when trying to export.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Building the shell along with its arguments
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C " + build_ffmpeg_command_line(filepath_input_video, output_path, list_img_info); // RAJOUTER LE + " & pause "; pour debugger
                startInfo.CreateNoWindow = true;

                // Shell launch
                MessageBox.Show("Please wait while the program is exporting your video...", "Wait");
                Process shellProcess = new Process() { StartInfo = startInfo };
                shellProcess.Start();
                shellProcess.WaitForExit();
                MessageBox.Show("Your export is done.", "Congratulations!");

                MessageBox.Show(build_ffmpeg_command_line(filepath_input_video, output_path, list_img_info));

            }
        }

        private string build_ffmpeg_command_line(string input_video_path, string result_video_path, List<ImagesInformation> list)
        {
            if (list.Count < 1)
                return "";

            if (input_video_path.Contains(" "))
                input_video_path = "\"" + input_video_path + "\"";

            string command_line = "ffmpeg.exe -y -i " + input_video_path + " ";

            //-i Path/to/the/picture.png
            for (int i = 0; i < list.Count; i++)
                command_line += "-i " + list[i].Filepath + " ";

            //***            
            command_line += "-filter_complex \"";

            //[1:v]scale=75:-1[Resultat0];
            for (int i = 0; i < list.Count; i++)
                command_line += "[" + (i + 1).ToString() + ":v]scale=" + list[i].PixelWidth + ":" + list[i].PixelHeight + "[Resultat" + i + "];";

            command_line += "[0:v][Resultat0]overlay=" + list[0].Position + ":enable='between(t," + list[0].StartTime + "," + list[0].StopTime + ")'";

            //[out1];[out1][Resultat1]overlay=10:10:enable='between(t,14.215,16.500)'
            for (int i = 1; i < list.Count; i++)
                command_line += "[out" + i + "];[out" + i + "][Resultat" + i + "]overlay=" + list[i].Position + ":enable='between(t," + list[i].StartTime + "," + list[i].StopTime + ")'";
                
            command_line += "\" -pix_fmt yuv420p " + result_video_path;

            return command_line;
        }

        /***********************************************************/
        /// 
        /// 
        ///               ATTRIBUTES    DECLARATION
        /// 
        /// 
        /***********************************************************/

        bool isPlaying = false;
        bool isSliderMouseClicked = false;

        Button[] lsButton = { };

        float periode_frame = 1000 / nb_img_sec;

        int nb_max_chords_on_width = 8;
        int offset = 5;
        int pixel_height = -1;
        int pixel_width = -1;        
        int selected_button = -1;

        List<ImagesInformation> list_img_info = new();

        Microsoft.Win32.OpenFileDialog filepath_input_video_dialog       = new() { InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Filter = "Videos (*.avi, *.mp4, *.wmv, *.mov)|*avi;*.mp4;*.wmv;*.mov" };
        Microsoft.Win32.OpenFileDialog filepath_picture_to_insert_dialog = new() { InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Filter = "Pictures (*.jpeg, *.jpg, *.png, *.bmp)|*jpeg;*.jpg;*.bmp;*.png" };
        Microsoft.Win32.SaveFileDialog output_video_directory_dialog     = new() { InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Filter = "Video (*.mp4, *.avi, *.wmv, *.mov)|*.mp4;*.avi;*.wmv;*.mov"};

        static int nb_img_sec = 30;
        
        string filepath_input_video = "";
        string filepath_input_picture = "";
        string filepath_output_directory = "";
        string filepath_output_filename = "";
        string position = "0:0";

        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        
        /*******************************/
        /* VITAL FOR LOADING THE ICONES*/
        /*******************************/
        string imgs_root_dir = @"";
    }

    /*********************************************************************************************************************************************************/
    /************************************************ CLASS THAT WILL CONTAIN THE VIDEO INLAYING INFORMATION *************************************************/
    /*********************************************************************************************************************************************************/
    public class ImagesInformation
    {
        public string _filepath;
        public string _pixel_width;
        public string _pixel_height;
        public string _position;
        public string _start_time;
        public string _stop_time;

        public ImagesInformation()
        {
            _filepath = "";
            _pixel_width = "";
            _pixel_height = "";
            _position = "";
            _start_time = "";
            _stop_time = "";
        }

        public ImagesInformation(string filepath, string pixel_width, string pixel_height, string position, string start_time, string stop_time)
        {
            _filepath = filepath;
            _pixel_width = pixel_width;
            _pixel_height = pixel_height;
            _position = position;
            _start_time = start_time;
            _stop_time = stop_time;
        }

        public string Filepath
        {
            get { return _filepath; }
            set { _filepath = value; }
        }

        public string PixelWidth
        {
            get { return _pixel_width; }
            set { _pixel_width = value; }
        }

        public string PixelHeight
        {
            get { return _pixel_height; }
            set { _pixel_height = value; }
        }

        public string Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public string StartTime
        {
            get { return _start_time; }
            set { _start_time = value; }
        }

        public string StopTime
        {
            get { return _stop_time; }
            set { _stop_time = value; }
        }
    }
}