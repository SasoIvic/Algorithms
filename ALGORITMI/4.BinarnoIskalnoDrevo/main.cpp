#include <iostream>
#include <iomanip>

using namespace std;

struct Drevo{
    int num;
    Drevo* oce;
    Drevo* leviSin;
    Drevo* desniSin;
};

void meni(){
	cout << "==========================================\n";
	cout << "MENU: \n";
	cout << "1 ... VSTAVI STEVILO\n";
	cout << "2 ... ISCIMO STEVILO\n";
	cout << "3 ... IZPISI DREVO\n";
	cout << "4 ... IZPIS POVEZAV\n";
	cout << "5 ... MINIMUMM\n";
	cout << "6 ... MAKSIMUM\n";
	cout << "7 ... NASLEDNJIK in PREDHODNIK\n";
	cout << "8 ... BRISANJE ELEMENTA\n";
	cout << "9 ... KONEC\n";
	cout << "===========================================\n";
	cout << "Izberi: ";
}

Drevo* dodajElement(Drevo* head, int stevilo, Drevo* oce){

    if(head==NULL && oce==NULL){ // ce je drevo prazno
        Drevo *novoDrevo=new Drevo();
        novoDrevo->num=stevilo;
        novoDrevo->oce=NULL;
        novoDrevo->leviSin=NULL;
        novoDrevo->desniSin=NULL;
        return novoDrevo;
    }
    else if(head==NULL){
        Drevo *novoDrevo=new Drevo();
        novoDrevo->num=stevilo;
        novoDrevo->oce=oce;
        novoDrevo->leviSin=NULL;
        novoDrevo->desniSin=NULL;
        return novoDrevo;
    }
    else if(stevilo > head->num){ // desni sin(vecji od oceta)
        head->desniSin=dodajElement(head->desniSin, stevilo, head);
    }
    else if(stevilo < head->num){ // levi sin(manjsi od oceta)
        head->leviSin=dodajElement(head->leviSin, stevilo, head);
    }
    else{
		cout << "To stevilo je ze v drevesu."<<endl;
	}
	return head;
}

Drevo* iskanjeElementa(Drevo* head, int iskano){
    if(head==NULL) // ko je prazno
        return head;
    else if (iskano==head->num)
		return head;
    else{
        if(iskano<head->num)
            iskanjeElementa(head->leviSin, iskano);//gremo levo
        else
            iskanjeElementa(head->desniSin, iskano);//gremo desno
    }
}

void izpisiDrevo(Drevo* head){
    if(head==NULL)//prazno
        return;
    izpisiDrevo(head->leviSin);// idi levo (najprej levo ker je manjse)
    cout<<head->num<<" "<<endl;
    izpisiDrevo(head->desniSin);// idi desno
}

void izpisPovezav(Drevo* head){
    if(head->leviSin!=NULL){
        cout << head->num << "-" << head->leviSin->num << endl;
        izpisPovezav(head->leviSin);
    }
    if(head->desniSin!=NULL){
        cout << head->num << "-" << head->desniSin->num << endl;
        izpisPovezav(head->desniSin);
    }
}

Drevo* minimum(Drevo* head){
    if (head == NULL){ // ce je prazen
		cout << "Seznam je prazen." << endl;
		return NULL;
	}
	else if (head->desniSin == NULL) // ni desnih
		return head;
    else{
        while(head->leviSin!=NULL){ // isce najvecjo
            head=head->leviSin;
        }
        return head;
    }
}
Drevo* maksimum(Drevo* head){
    if (head == NULL){ // ce je prazen
		cout << "Seznam je prazen." << endl;
		return NULL;
	}
	else if (head->leviSin == NULL) // ni levih
		return head;
    else{
        while(head->desniSin!=NULL){ // isce najmanjso
            head=head->desniSin;
        }
        return head;
    }
}

Drevo* predhodnik(Drevo* a){
    if(a->leviSin!=NULL){
        return maksimum(a->leviSin);//najvecji med manjsimi
    }

    Drevo* prejsnji = a->oce;
    while(prejsnji!=NULL && a==prejsnji->leviSin){
        a=prejsnji;
        prejsnji=prejsnji->oce;
    }
    return prejsnji;
}
Drevo* naslednjik(Drevo* a){
    if(a->desniSin!=NULL){
        return minimum(a->desniSin);// najmanjsi med najvecjimi
    }

    Drevo* prejsnji = a->oce;
    while(prejsnji!=NULL && a==prejsnji->desniSin){
        a=prejsnji;
        prejsnji=prejsnji->oce;
    }
    return prejsnji;
}
Drevo* brisanjeElementa(Drevo** head, int iskano){

    Drevo* izbrisi = iskanjeElementa(*head, iskano);

    if(izbrisi!=NULL){
        Drevo* y;
        Drevo* x;

        if(izbrisi->leviSin==NULL || izbrisi->desniSin==NULL)//vozlisce z enim sinovom
            y=izbrisi;
        else // vozlisce z dvema sinovoma
            y=naslednjik(izbrisi);

        if(y->leviSin==NULL)
            x=y->desniSin;
        else
            x=y->leviSin;

        if(x!=NULL)
            x->oce=y->oce;

        if(y->oce==NULL)
            *head=x;
        else if(y == y->oce->leviSin)
            y->oce->leviSin=x;
        else
            y->oce->desniSin=x;

        if(y != izbrisi) // korena ne brisemo ampak podatek premaknemo v koren
            izbrisi->num = y->num;

        delete y;
        return y;
    }
    else
        cout<<"Ni stevila za izbris."<<endl;
}


int main()
{
    Drevo* head=NULL;
    Drevo* iskaniEl=NULL;
    Drevo* a=NULL;

    int iskano;
    char izbira;
    bool running=true;
    do{
        meni();
        cin>>izbira;
		switch (izbira) {
		    case '1':
		        int stevilo;
		        cout<<"Vstavi stevilo: ";
		        cin>>stevilo;
		        head=dodajElement(head, stevilo, NULL);
                break;

            case '2':
                cout<<"Katero stevilo isces? ";
		        cin>>iskano;
                iskaniEl=iskanjeElementa(head, iskano);
                if(iskaniEl==NULL){
                    cout<<"Iskanega elementa ni v seznamu."<<endl;
                }
                else{
                    cout<<"Iskani element, "<<iskaniEl->num<<", je v seznamu."<<endl;
                }
                break;

            case '3':
                izpisiDrevo(head);
                cout<<endl;
                break;

            case '4':
                cout<<"Povezave med elementi: "<<endl;
                if(head!=NULL)
                    izpisPovezav(head);
                else
                    cout<<"Seznam je prazen."<<endl;
                break;

            case '5':
                if(head!=NULL)
                    cout<<"minimum: "<<minimum(head)->num<<endl;
                else
                    cout<<"Seznam je prazen."<<endl;
                break;

            case '6':
                if(head!=NULL)
                    cout<<"maxmum: "<<maksimum(head)->num<<endl;
                else
                    cout<<"Seznam je prazen."<<endl;
                break;

            case '7':
                cout << "Vpisi stevilo za iskanje predhodnjika in naslednjika: ";
                cin >> iskano;
                a=iskanjeElementa(head, iskano);
                if(a!=NULL){
                    if(predhodnik(a)==NULL)
                        cout<<"Stevilo nima predhodjika."<<endl;
                    else
                        cout<< "Predhodnjik je: "<<predhodnik(a)->num<<endl;

                    if(naslednjik(a)==NULL)
                        cout<<"Stevilo nima naslednjika."<<endl;
                    else
                        cout<< "Naslednjik je: "<<naslednjik(a)->num<<endl;
                }
                else
                    cout<<"Ni tega stevila."<<endl;
                break;

            case '8':
                cout << "Vpisi stevilo ki ga zelis izbrisati: ";
                cin >> iskano;
                brisanjeElementa(&head, iskano);
                break;

            case '9':
                running = false;
                break;

            default:
                cout << "Napacna izbira!\n";
                break;
		}
        } while (running);


    return 0;
}
