CMosaic  (c) mjt, 2015-2016  [mixut@hotmail.com]

Released under MIT-license.

Compiled .exes can be found under CMosaic\bin\Debug\ directory.


There are two versions of CMosaic:
 CMosaic   (older)
    usage:  mainImage  [imagesDir (ONLY jpg files used)]
	
	
and
  CMosaic_UseThreads
	usage:  mainImage  [imagesDir (ONLY jpg files used)] [number_of_threads] [block_width] [block_height]
	
 mainImage    - mosaic image.
 imagesDir    - directory which contains lots of .jpg images
 
 
 
 Usage examples:
  CMosaic.exe  myUglyFace.jpg  imagesDir
  CMosaic_UseThreads.exe  myFace.jpg  imagesDir  2  32 32

