# Galaxy Morphology 

Code for the website www.galaxyml.uk and for the related MNRAS paper: Hocking, A et al. "An automatic taxonomy of galaxy morphology using unsupervised machine learning" MNRAS (2018)

The catalogue of CANDELS that was created by the technique and described in the paper can be found here: 
[catalogue](https://github.com/alexhock/galaxymorphology/tree/master/catalogue)


This repository also contains source code for multi-threaded versions of several machine learning algorithms.

The C# code analyses FITS survey images.

The website is a Java Spring Boot web app with REST api for galaxy similarity search. The website uses an in-memory cache of the galaxy catalogue for performance. 

The C# source code can be compiled with Visual Studio 2015.

For the Java site & REST api I use IntelliJ.


