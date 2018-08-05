# CANDELS Catalogue

The CSV catalog data of CANDELS galaxies classifications that was used to show the galaxies on the www.galaxyml.uk site can be downloaded from here: 

[https://github.com/alexhock/galaxymorphology/catalogue/ml-candels-classifications.csv](https://github.com/alexhock/galaxymorphology/catalogue/ml-candels-classifications.csv)

The classifications that are best used in combination with the similarities column that you see described in the table below. First identify the classification you are interested in and then sort the galaxies using the similarity value. The smaller the corresponding similarity value the most relevant the galaxy is to the classification. In order to produce the final catalog we used Astropy to match the unsupervised machine learning catalogue with the catalogue from the 3D HST WFC3 selected photometric catalogue from Skelton 2014 [1]. The table below contains the description of the catalogue file. The catalogue is in CSV format. 

## Catalogue Columns

<table>
    <thead>
      <tr>
        <th>Column Position</th>
        <th>Column Name</th>
        <th>Description</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>1</td>
        <td>Field Id</td>
        <td>The identifier of the field where the object resides. 0 GOODS-N, 1 UDS, 2 EGS, 3 COSMOS, 4 GOODS-S</td>
      </tr>
      <tr>
        <td>2</td>
        <td>Object Id</td>
        <td>The ID of the object equivalent to the 3d catalog by Skelton 2014 [1]</td>
      </tr>
      <tr>
        <td>3</td>
        <td>RA (degrees)</td>
        <td></td>
      </tr>
      <tr>
        <td>4</td>
        <td>Dec (degrees)</td>
        <td></td>
      </tr>
      <tr>
        <td>5-10</td>
        <td>Classifications</td>
        <td>Hierarchical classifications, 6 levels of classifications</td>
      </tr>
      <tr>
        <td>11-16</td>
        <td>Classification similarities</td>
        <td>A number between 0 and 1. The nearer to 0 the nearer the object is to the cluster centre. These fields are important for sorting the objects within classifications.</td>
      </tr>
    </tbody>
</table>


[1] Skelton, Rosalind E., et al. "3D-HST WFC3-selected Photometric Catalogs in the Five CANDELS/3D-HST Fields: Photometry, Photometric Redshifts, and Stellar Masses." The Astrophysical Journal Supplement Series 214.2 (2014): 24.