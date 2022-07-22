#include <iostream>
#include <cstdlib>
#include <ctime>
#include <vector>
#include <bitset>
#include <string>
#include <stdlib.h>
#include <fstream>
#include<sstream>
using namespace std;

void uredi (vector<string> &bin){
    int i=1;
    int k=7;
    int j;
    while(k!=-1){
        while(i<bin.size()){
            j=i;
            while(j>0 && bin[j-1][k]<bin[j][k]){
                swap(bin[j], bin[j-1]);
                j--;
            }
            i++;
        }
        k--;
        i=1;
    }
}

int main(int argc, char *argv[])
{
    if(argv[1]==NULL){
        cout<<"RadixSort <vhodna datoteka> "<<endl;
        cout<<"     <vhodna datoteka>: pot do vhodne datoteke"<<endl;
        return 0;
    }
    string pot=argv[1]; // vnesi pot do datoteke s podatki

    ifstream file(pot.c_str());
    vector<int> num;
    int numbers;
    while(file >> numbers){
        num.push_back(numbers);
    }
    vector<string> binaryP; // za pozitivne
    vector<string> binaryN; // za negativne

    for(int i=0; i<num.size(); i++){
        if(num[i]>=0){
            binaryP.push_back(bitset<8>(num[i]).to_string());
        }
        else{
            binaryN.push_back(bitset<8>(num[i]).to_string());
        }
    }

    uredi(binaryN);
    uredi(binaryP);

    ofstream izpis("out.txt"); // ustvari datoteko

    izpis<<"UREJENA STEVILA: "<<endl;

    for(int i=0; i<binaryP.size(); i++){
        izpis<<bitset<8>(binaryP[i]).to_ulong()<<" ";
    }
    int a[binaryN.size()];
    for(int i=0; i<binaryN.size(); i++){
        a[i] = bitset<8>(binaryN[i]).to_ulong() -256;
        izpis<<a[i]<<" ";
    }

    return 0;
}
