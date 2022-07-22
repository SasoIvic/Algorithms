#include <iostream>
#include <string>

using namespace std;


int main()
{
    int arraySize;
    int num;
    int vrh[3] = {0,0,0}; // vsi trije vrhovi so 0

    cout<<"Vnesi velikost stolpa."<<endl;
    cin>>arraySize;
    while(arraySize<=1){
        cout<<"Velikost mora biti vecja od 1! Vnesi velikost stolpa."<<endl;
        cin>>arraySize;
    }

    int polje[3][arraySize]; // Ustvari polja za 3 stolpe
    int j=0;
    for(int i=arraySize; i>0; i--){ // napolni polja (zacetek)
        polje[0][j] =i;
        j++;
        vrh[0]++;
    }

     for(int i=0; i<3; i++){
            for(int j=0; j<vrh[i]; j++){
                cout<<polje[i][j]<<" ";
            }
            cout<<endl;
    }

    int izbranStolpec1; // Prestavi iz ... na
    int izbranStolpec2;
    bool izbira;

    while(vrh[3]==arraySize){ // dokler ni 3. poln ...(KONEC)
        izbira=false;
        while (izbira == false){
            cout << "Prestavi iz: ";
            cin >> izbranStolpec1;
            cout << "Prestavi na: ";
            cin >> izbranStolpec2;

            if (izbranStolpec1>0 && izbranStolpec1<4 && izbranStolpec2>0 && izbranStolpec2<4 && izbranStolpec1!=izbranStolpec2)
                izbira = true;
            else
                cout<<"\nNeveljavna izbira!\n";
        }

        if(vrh[izbranStolpec1-1]==0)//Ta stolpec je prazen
        {
            cout<<"Napaka. Stolpec " <<izbranStolpec1<<" je prazen."<<endl;
        }


        if(vrh[izbranStolpec2-1]==0 && vrh[izbranStolpec1-1]!=0)//Ce je drugi prazen
        {
           // cout<<"vrhec1 "<<vrh[izbranStolpec1-1]<<endl;
           // cout<<"vrhec2 "<<vrh[izbranStolpec2-1]<<endl;
            vrh[izbranStolpec1-1]--;
            polje[izbranStolpec2-1][vrh[izbranStolpec2-1]] = polje[izbranStolpec1-1][vrh[izbranStolpec1-1]];
            vrh[izbranStolpec2-1]++;

        }

        else if(vrh[izbranStolpec1-1]!=0 && vrh[izbranStolpec2-1]!=0)//Ce ni noben prazen
        {
          //  cout << "premikam:" << polje[izbranStolpec1-1][vrh[izbranStolpec1-1]-1] << " na:" << polje[izbranStolpec2-1][vrh[izbranStolpec2-1]-1] << endl;
            if( polje[izbranStolpec2-1][vrh[izbranStolpec2-1]-1] > polje[izbranStolpec1-1][vrh[izbranStolpec1-1]-1])
            {
                vrh[izbranStolpec1-1]--;
                polje[izbranStolpec2-1][vrh[izbranStolpec2-1]] = polje[izbranStolpec1-1][vrh[izbranStolpec1-1]];
                vrh[izbranStolpec2-1]++;

            }
            else
            {
            cout<<"Napaka. Prestavite lahko le manjse stevilo na vecje."<<endl;
            }
        }

        for(int i=0; i<3; i++){
            for(int j=0; j<vrh[i]; j++){
                cout<<polje[i][j]<<" ";
            }
            cout<<endl;
        }
      //  cout << vrh[0] << " " << vrh[1] << " " << vrh[2] << endl;

    }
    cout<<"Cestitamo. Koncali ste igro."<<endl;

    return 0;
}
