using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThoughtWorks.QRCode.Codec;
using System.IO;
using System.Drawing;
namespace KMBit.Util
{
    public class QRCodeUtil
    {
        public static Bitmap Create_ImgCode(string codeNumber, int size)
        {
            //创建二维码生成类
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            //设置编码模式
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            //设置编码测量度
            qrCodeEncoder.QRCodeScale = size;
            //设置编码版本
            qrCodeEncoder.QRCodeVersion = 0;
            //设置编码错误纠正
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            //生成二维码图片
            System.Drawing.Bitmap image = qrCodeEncoder.Encode(codeNumber);
            return image;
        }

        public static void SaveImg(string strPath,string fileName, Bitmap img)
        {
            //保存图片到目录
            try
            {
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }

                img.Save(Path.Combine(strPath,fileName), System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {

            }
            finally { }            
        }

        public static void CreateQRCode(string path,string fileName,string codeContent,int size=6)
        {
            Bitmap bit = Create_ImgCode(codeContent, size);
            SaveImg(path,fileName,bit);
        }
    }
}
