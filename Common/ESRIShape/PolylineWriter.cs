using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRIShape.Entity;

namespace ESRIShape
{
    public class PolylineWriter : Writer
    {
        private int _contentLength;

        protected override int ContentLength
        {
            get
            {
                return _contentLength;
            }
            set
            {
                _contentLength = value;
            }
        }

        public PolylineWriter(string filePath, string fileName, List<FieldInfo> listField)
            :base(filePath,fileName,listField)
        {
        }

        /// <summary>
        /// 将实体位置信息写入shp文件
        /// </summary>
        /// <param name="feature"></param>
        protected override void WriteFeatureToShp(Feature feature)
        {
            PolyLineFeature plineFeature = feature as PolyLineFeature;
            _contentLength = plineFeature.GetContentLength();

            byte[] value = null;

            //记录号与内容长度
            value = Helper.IntToBytes(Helper.BigAndLittleSwitch((uint)RecordNum));
            shpFileStream.Write(value, 0, 4);
            value = Helper.IntToBytes(Helper.BigAndLittleSwitch((uint)_contentLength));
            shpFileStream.Write(value, 0, 4);

            //1-几何类型
            value = Helper.IntToBytes(plineFeature.ShapeType);
            shpFileStream.Write(value, 0, 4);

            //2-坐标范围
            value = Helper.DoubleToBytes(plineFeature.Box[0]);
            shpFileStream.Write(value, 0, 8);
            value = Helper.DoubleToBytes(plineFeature.Box[1]);
            shpFileStream.Write(value, 0, 8);
            value = Helper.DoubleToBytes(plineFeature.Box[2]);
            shpFileStream.Write(value, 0, 8);
            value = Helper.DoubleToBytes(plineFeature.Box[3]);
            shpFileStream.Write(value, 0, 8);

            //3-子线段个数
            value = Helper.IntToBytes(plineFeature.NumParts);
            shpFileStream.Write(value, 0, 4);

            //4-线所包含的点数
            value = Helper.IntToBytes(plineFeature.NumPoints);
            shpFileStream.Write(value, 0, 4);

            //5-表示在Points中每个子线段的起点坐标位置-大小为NumPoints
            for (int i = 0; i < plineFeature.NumParts; ++i)
            {
                value = Helper.IntToBytes(plineFeature.Parts[i]);
                shpFileStream.Write(value, 0, 4);
            }

            //6-记录了所有坐标信息
            for (int i = 0; i < plineFeature.NumPoints; ++i)
            {
                value = Helper.DoubleToBytes(plineFeature.Points[i].X);
                shpFileStream.Write(value, 0, 8);
                value = Helper.DoubleToBytes(plineFeature.Points[i].Y);
                shpFileStream.Write(value, 0, 8);
            }
        }

    }
}
