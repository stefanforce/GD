<!DOCTYPE html>
<html>
<body>
<h1>Raport 5</h1>
<h1>Tiperciuc Stefan (3A5)</h1>


<p>In acest raport voi prezenta continuarea progresului.</p>

<p>Saptamana aceasta am finalizat implementarea actiunilor ce pot fi facute de catre un jucator in tura.Acesta poate sa mute panda pentru a manca bambus(in mod corect, doar in linie dreapta), sa mute fermierul(in acelasi mod) si sa creasca bambus cu ajutorul fermierului.</p>
<p>Cel mai mult timp alocat acestei saptamani a fost pentru implementarea cresterii bambusului. Atunci cand fermierul ajunge pe un tile, acesta creste nivelul de bambus pe acel tile si la tile-urile adiacente cu 1, pana la o valoare maxima de 4. Am implementat, de asemenea, functionalitatea panda-ului de a manca bambus atunci cand ajunge pe un anumit tile, valoare ce nu poate fi scazuta sub 0.</p> 
<p>Tot in aceasta saptamana am inceput sa fac infrastructura de turn-based a jocului, creandu-mi o variabila de tip enum ce retine care este starea curenta in care se afla jucatorul(aruncarea zarului pentru conditii meteo si cele 2/3 actiuni pe care le poate avea).</p>
</body>
</html>

<!DOCTYPE html>
<html>
<body>