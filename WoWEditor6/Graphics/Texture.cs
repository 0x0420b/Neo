﻿using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace WoWEditor6.Graphics
{
    class Texture
    {
        private static Texture2D gDefaultTexture;
        private static ShaderResourceView gDefaultView;

        private Texture2D mTexture;
        private readonly GxContext mContext;

        public ShaderResourceView NativeView { get; private set; }

        public Texture(GxContext context)
        {
            mContext = context;
            NativeView = gDefaultView;
            mTexture = gDefaultTexture;
        }

        public void UpdateTexture(int width, int height, Format format, List<byte[]> layers, List<int> rowSizes)
        {
            var boxes = new DataBox[layers.Count];
            var streams = new DataStream[layers.Count];
            try
            {
                for (var i = 0; i < layers.Count; ++i)
                {
                    streams[i] = new DataStream(layers[i].Length, true, true);
                    streams[i].WriteRange(layers[i]);
                    boxes[i] = new DataBox(streams[i].DataPointer, rowSizes[i], 0);
                }

                if (width != mTexture.Description.Width || height != mTexture.Description.Height ||
                    format != mTexture.Description.Format || mTexture.Description.MipLevels != layers.Count ||
                    mTexture == gDefaultTexture)
                {
                    CreateNew(width, height, format, boxes);
                }
                else
                {
                    for (var i = 0; i < layers.Count; ++i)
                    {
                        mContext.Context.UpdateSubresource(boxes[i], mTexture, i);
                    }
                }
            }
            finally
            {
                foreach (var strm in streams) strm?.Dispose();
            }
        }

        private void CreateNew(int width, int height, Format format, DataBox[] boxes)
        {
            if (mTexture != gDefaultTexture)
            {
                mTexture.Dispose();
                NativeView.Dispose();
            }

            var desc = mTexture.Description;
            desc.Width = width;
            desc.Height = height;
            desc.Format = format;
            mTexture = new Texture2D(mContext.Device, desc, boxes);

            var srvd = new ShaderResourceViewDescription
            {
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                Format = format,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = boxes.Length, MostDetailedMip = 0 }
            };

            NativeView = new ShaderResourceView(mContext.Device, mTexture, srvd);
        }

        public static void InitDefaultTexture(GxContext context)
        {
            var desc = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                Height = 2,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Immutable,
                Width = 2
            };

            using (var strm = new DataStream(16, true, true))
            {
                strm.WriteRange(new[] {0xFFFF0000, 0xFF00FF00, 0xFF0000FF, 0xFFFFFFFF});
                var layerData = new DataBox(strm.DataPointer) {RowPitch = 8};
                gDefaultTexture = new Texture2D(context.Device, desc, new[] { layerData });
            }

            var srvd = new ShaderResourceViewDescription
            {
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                Format = Format.R8G8B8A8_UNorm,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource
                {
                    MipLevels = 1,
                    MostDetailedMip = 0
                }
            };

            gDefaultView = new ShaderResourceView(context.Device, gDefaultTexture, srvd);
        }
    }
}