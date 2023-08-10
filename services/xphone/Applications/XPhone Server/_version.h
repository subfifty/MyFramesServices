#ifdef PATCH_BUILD
  #define FILEVER         9, 0, 150, 1167
  #define PRODUCTVER      9, 0, 167, 0
  #define STRFILEVER      "9, 0, 150, 1167\0"
  #define STRPRODUCTVER   "9, 0, 167, 0\0"
  #define STRPRIVATEBUILD "\0"
#else
  #define FILEVER         9, 0, 150, 167
  #define PRODUCTVER      9, 0, 167, 0
  #define STRFILEVER      "9, 0, 150, 167\0"
  #define STRPRODUCTVER   "9, 0, 167, 0\0"
  #define STRPRIVATEBUILD "\0"
#endif

//Look at Q238270
#define STRCOMPANY      "C4B Com For Business AG\0"
#define STRCOPYRIGHT    "© C4B Com For Business AG. Alle Rechte vorbehalten.\0"
