<!DOCTYPE html>
<html>
<body>
<h1>Raport 7</h1>
<h1>Tiperciuc Stefan (3A5)</h1>


<p>In acest raport voi prezenta progresul de pana in saptamana a 9-a.</p>
<p>In saptamana aceasta am implementat 2 lucruri: conditia meteo "random" si o noua modalitate de plasare a tile-urilor.</p>
<p>Primul lucru implementat a fost cea de-a 6-a fata a zarului conform jocului. Atunci cand jucatorul nimereste acea fata a zarului, el are posibilitatea sa selecteze oricare dintre celelalte 5 fete ale zarului si sa faca actiunea acestuia. In state-ul Weather1 este ales noul zar, iar in Weather2 este facuta actiunea acelui zar.</p>
<p>Al doilea lucru implementat a fost schimbarea modului in care jucatorul alege un tile ce trebuie plasat pe board. Anterior, ii erau oferite tile-uri fara nici un improvement in mod aleator.Saptamana aceasta am extras toate tile-urile din jocul initial, le-am adaugat ca si asset-uri jocului meu si am facut un json pentru ele. Json-ul contine toate informatiile necesare despre tile, precum nume,culoarea tile-ului si improvement-ul pe care il are de la inceput. Aceste json-urile le-am transferat intr-o clasa in C#, am facut o lista de ele(de tile-uri cu atribute) si le-am pus ca si noua metoda de extragere a unui tile. Acum, cand un jucator vrea sa ia un tile, ii vor fi prezentare 3 tile-uri aleatoare din acea lista si cand alege unul din ele, el va fi scos din lista initiala sa nu fie repetate. In lista sunt initial 23 de tile-uri. </p>

</body>
</html>

<!DOCTYPE html>
<html>
<body>