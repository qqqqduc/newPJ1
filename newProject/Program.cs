using newProject.Driver;
using SkiaSharp;

namespace newProject
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        /// 
        static HandleProduct _handleProduct = new HandleProduct();

        [STAThread]
        
        static void Main()
        {
            _handleProduct.Start();

            // Đường dẫn tới model và ảnh
            string modelPath = @"yolov11_s_pretrained_1(a).onnx";
            string imagePath = @"test_image.jpg";
            string outputPath = @"result_image.jpg";

            // Khởi tạo YoloDetector
            using var detector = new YoloDetector(modelPath, useCuda: false);

            // Load image
            using var image = SKImage.FromEncodedData(imagePath);

            // Chạy inference
            var (detections, classCounts) = detector.RunInference(image, confidenceThreshold: 0.7, iouThreshold: 0.7);

            // In số lượng box cho mỗi class
            Console.WriteLine("\nCount number objects of classes:");
            foreach (var pair in classCounts)
            {
                Console.WriteLine($"Class: {pair.Key}, Count: {pair.Value}");
            }

            // Lưu ảnh kết quả
            detector.SaveResultImage(imagePath, detections, outputPath);

            Console.WriteLine("Complete!");
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainView());
        }
    }
}