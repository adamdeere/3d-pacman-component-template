﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;


namespace OpenGL_Game.Objects
{
    class TextureObject
    {
        private string m_Name;
        private string m_Tag;
        private int m_TextureNumber;
        private Dictionary<string, int> textureDictionary = new Dictionary<string, int>();
      
       
        public TextureObject(string tag, string filename)
        {
            m_Tag = tag;
            m_Name = filename;
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException(filename);

            textureDictionary.TryGetValue(filename, out int texture);
            if (texture == 0)
            {
                Bitmap bmp = new Bitmap(filename);
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
              
                GL.GenTextures(1, out m_TextureNumber);
                GL.BindTexture(TextureTarget.Texture2D, m_TextureNumber);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                   OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                // We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
                // We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
                // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
              
                bmp.UnlockBits(bmp_data);
            }
        }
        public int GetTextureNumber => m_TextureNumber;

        public string GetTextureTag => m_Tag;
       
       
    }
}
