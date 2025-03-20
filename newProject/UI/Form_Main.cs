using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Basler.Pylon;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using static System.Resources.ResXFileRef;
using EasyModbus;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing.Text;
using EasyModbusV2;

namespace newProject.UI
{
    public partial class Form_Main : UserControl
    {
        //string modelPath = @"C:\Users\phucn\OneDrive\Documents\YOLO_Csharp\test_yolo\yolov11_s_pretrained_1(a).onnx";
        //string imagePath = @"C:\Users\phucn\OneDrive\Documents\YOLO_Csharp\test_yolo\test_image.jpg";
        //string outputPath = @"C:\Users\phucn\OneDrive\Documents\YOLO_Csharp\test_yolo\result_image.jpg";
        //// Khởi tạo YoloDetector
        //YoloDetector detector = new YoloDetector(modelPath, useCuda: false);

        //// Load image
        //var image = SKImage.FromEncodedData(imagePath);

        //// Chạy inference
        //var (detections, classCounts) = detector.RunInference(image, confidenceThreshold: 0.7, iouThreshold: 0.7);

        //// In số lượng box cho mỗi class
        //Console.WriteLine("\nCount number objects of classes:");
        //foreach (var pair in classCounts)
        //{
        //    Console.WriteLine($"Class: {pair.Key}, Count: {pair.Value}");
        //}

        //// Lưu ảnh kết quả
        //detector.SaveResultImage(imagePath, detections, outputPath);

        //Console.WriteLine("Complete!");

        private Camera camera;          //Object Camera
        private Bitmap bitmap;          //Data image
        private bool isGrabbing = false;    //Grab status
        PixelDataConverter converter = new PixelDataConverter();
        
        //private HandleProduct _handleProduct = new HandleProduct();

        public Form_Main()
        {
            InitializeComponent();

        }

        public void UpdateUI(string result, int StartTime, int RunTime, int FinishTime, int countOK, int countNG)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    if (result == "OK")
                    {
                        countOK++;
                    }
                    else if (result == "NG")
                    {
                        countNG++;
                    }
                    rjButton4.Text = Convert.ToString(StartTime);
                    rjButton5.Text = Convert.ToString(RunTime);
                    rjButton7.Text = Convert.ToString(FinishTime);
                    rjButton10.Text = Convert.ToString(countOK + countNG);
                    rjButton11.Text = Convert.ToString(countOK);
                    rjButton12.Text = Convert.ToString(countNG);
                }));
            }
            else
            {
                /////
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                camera = new Camera(); // Tạo đối tượng camera mặc định
                // Set the acquisition mode to free running continuous acquisition when the camera is opened.
                camera.CameraOpened += Configuration.AcquireContinuous;
                camera.Open(); // Mở kết nối camera
                camera.Parameters[PLCamera.ExposureAuto].SetValue("Continuous");
                //camera.Parameters[PLCamera.ExposureTime].SetValue(150000);
                MessageBox.Show("Kết nối camera thành công!", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kết nối camera: {ex.Message}", "Lỗi");
            }
        }

        private void OnImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            if (!isGrabbing) return;

            try
            {
                IGrabResult grabResult = e.GrabResult;
                if (grabResult.GrabSucceeded)
                {
                    //ImageWindow.DisplayImage(0, grabResult);

                    // Convert the image to a Bitmap
                    if (bitmap == null || bitmap.Width != grabResult.Width || bitmap.Height != grabResult.Height)
                    {
                        bitmap = new Bitmap(grabResult.Width, grabResult.Height, PixelFormat.Format24bppRgb);
                    }

                    BitmapData bitmapData = null;
                    try
                    {
                        bitmapData = bitmap.LockBits(
                            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                            ImageLockMode.WriteOnly,
                            bitmap.PixelFormat);

                        // Convert the pixel data from the camera to the bitmap format
                        converter.OutputPixelFormat = PixelType.BGR8packed;
                        converter.Convert(bitmapData.Scan0,
                                          bitmapData.Stride * bitmapData.Height,
                                          grabResult);
                    }
                    finally
                    {
                        if (bitmapData != null)
                        {
                            bitmap.UnlockBits(bitmapData);
                        }
                    }
                    pictureBoxCamera.Image = (Bitmap)bitmap.Clone();  // Hiển thị hình ảnh
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xử lý hình ảnh: {ex.Message}", "Lỗi");
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (camera == null || !camera.IsOpen)
            {
                MessageBox.Show("Camera chưa được kết nối!", "Cảnh báo");
                return;
            }
            isGrabbing = true;
            camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
            camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (camera != null && camera.StreamGrabber.IsGrabbing)
            {
                isGrabbing = false;
                camera.StreamGrabber.Stop(); // Dừng thu hình
                MessageBox.Show("Stopped grabbing images.");
            }
        }

        public void MainForm_FormClosing()
        {
            if (camera != null)
            {
                camera.Close();
                camera.Dispose();
            }
        }

        private void buttonShoot_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra xem PictureBox có ảnh hay không
                if (pictureBoxCamera.Image != null)
                {
                    // Đường dẫn đến thư mục trong project
                    string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Capture");

                    // Kiểm tra và tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // Đặt tên file và đường dẫn file
                    string filePath = Path.Combine(folderPath, "CapturedImage_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");

                    // Lưu ảnh từ PictureBox vào file
                    pictureBoxCamera.Image.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

                    // Thông báo thành công
                    MessageBox.Show($"Image saved successfully at: {filePath}", "Capture", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No image to capture!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                MessageBox.Show($"Error capturing image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }



    //public class HandleProduct
    //{
    //    
    //}
}
