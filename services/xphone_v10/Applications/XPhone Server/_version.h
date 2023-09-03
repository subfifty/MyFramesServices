#ifdef PATCH_BUILD
  #define FILEVER         9, 0, 2000, 4009
  #define PRODUCTVER      9, 0, 3009, 0
  #define STRFILEVER      "9, 0, 2000, 4009\0"
  #define STRPRODUCTVER   "9, 0, 3009, 0\0"
  #define STRPRIVATEBUILD "\0"
#else
  #define FILEVER         9, 0, 2000, 1009
  #define PRODUCTVER      9, 0, 3009, 0
  #define STRFILEVER      "9, 0, 2000, 1009\0"
  #define STRPRODUCTVER   "9, 0, 3009, 0\0"
  #define STRPRIVATEBUILD "\0"
#endif

//Look at Q238270
#define STRCOMPANY      "C4B Com For Business AG\0"
#define STRCOPYRIGHT    "© C4B Com For Business AG. Alle Rechte vorbehalten.\0"
