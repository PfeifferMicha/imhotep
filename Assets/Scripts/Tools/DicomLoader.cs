using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gdcm;

public class DicomLoader {
    public DicomLoader( string directory )
    {
        Debug.Log("Loading Dicoms from: " + directory);
        Tag tagStudyInstanceUID = new Tag(0x20, 0x000d);
        Tag tagSeriesInstanceUID = new Tag(0x20, 0x000e);

        Directory d = new Directory();
        uint nfiles = d.Load(directory);
        if (nfiles == 0) return;
        //Debug.Log( "Files:\n" + d.toString() );

        // Use a StrictScanner, need to use a reference to pass the C++ pointer to
        // MyWatcher implementation
        SmartPtrStrictScan sscan = StrictScanner.New();
        StrictScanner s = sscan.__ref__();

        s.AddTag(tagStudyInstanceUID);
        s.AddTag(tagSeriesInstanceUID);
        bool b = s.Scan(d.GetFilenames());
        if (!b) return;

        ValuesType vals = s.GetValues();

        Debug.Log(vals.ToString());


        Image img = null;

        var reader = new gdcm.ImageReader();
        for (int i = 10; i < (int)nfiles; ++i)
        {
            if ( s.IsKey(d.GetFilenames()[i]) )     // If the tags exist, this is a valid DICOM, so we can read it!
            {
                Debug.Log(i);
                reader.SetFileName(d.GetFilenames()[i]);
                if (reader.Read())
                {
                    Debug.Log("Reading: " + d.GetFilenames()[i]);
                    img = reader.GetImage();
                    PixelFormat sourcePF = img.GetPixelFormat();
                    int width = (int)img.GetDimension(0);
                    int height = (int)img.GetDimension(1);

                    Debug.Log("Format: " + sourcePF.GetScalarTypeAsString());
                    Debug.Log("Buffer Size: " + (int)img.GetBufferLength());
                    Debug.Log("widht: " + width);
                    Debug.Log("height: " + height);
                    
                    byte[] buffer = new byte[img.GetBufferLength()];
                    img.GetBuffer(buffer);


                    Texture2D tex = new Texture2D( width, height, TextureFormat.R16, false );
                    tex.LoadRawTextureData(buffer);
                    /*
                    UInt16 min = 9999, max = 0;
                    float minf = 9999, maxf = 0;
                    int x = 0, y = 0;
                    for( int pixel = 0; pixel < img.GetBufferLength() ; pixel += 2 )
                    {
                        UInt16 val = BitConverter.ToUInt16(buffer, pixel);
                        if (val < min) min = val;
                        if (val > max) max = val;
                        float valF = (float) val / (float) ushort.MaxValue;
                        if (valF > maxf) maxf = valF;
                        if (valF < minf) minf = valF;
                        tex.SetPixel(x, y, new Color(valF, valF, valF));
                        x ++;
                        if (x >= width)
                        {
                            y++;
                            x = 0;
                            if (y >= height)
                                break;
                        }
                    }
                    Debug.Log("min,max: " + min + " " + max);
                    Debug.Log("min,max: " + minf + " " + maxf);*/
                    tex.Apply();

                    GameObject dicomViewer = GameObject.Find("DICOM_Plane");
                    if( dicomViewer )
                    {
                        Renderer dicomRenderer = dicomViewer.GetComponent<Renderer>();
                        dicomRenderer.material.mainTexture = tex;
                    } else { Debug.Log("Can't find obj"); }

                    break;      // Only load one texture!!
                }
            }
        }

        /*if (img != null)
        {
            try
            {

                //byte[] buffer = new byte[img.GetBufferLength()];
                //img.GetBuffer(buffer);
                Debug.Log("Length: " + img.GetBufferLength());
                Debug.Log("wdith: " + (int)img.GetDimension(0) + " height: " + (int)img.GetDimension(1));

                //Texture2D tex = new Texture2D((int)img.GetDimension(0), (int)img.GetDimension(1));
                //tex.LoadRawTextureData(buffer);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }*/

                    /*for (int i = 0; i < (int)nfiles; ++i)
                    {
                        if (!s.IsKey(d.GetFilenames()[i]))
                        {
                            System.Console.WriteLine("File is not DICOM or could not be read: " + d.GetFilenames()[i]);
                        }
                    }*/

                    /*bool initializedUIDs = false;
                    string seriesInstanceUID = "";
                    string studyInstanceUID = "";

                    string result = "";
                    for (int i = 0; i < (int)nfiles; ++i)
                    {
                        if (s.IsKey(d.GetFilenames()[i]))       // the tags could be read
                        {
                            if( initializedUIDs )
                            {
                                initializedUIDs = true;
                            }
                            var tagMap = s.GetMapping(d.GetFilenames()[i]);
                            tagMap(tagStudyInstanceUID);
                        }
                    }*/

                    //Debug.Log("Scan:\n" + s.toString());

                    //Debug.Log(result);
                }
            }
