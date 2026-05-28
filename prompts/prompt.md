## Scope

* Wir wollen die Speisekarte der Pizzeria Gabriel in einer JSON-Datei im Dateisystem abspeichern, da wir schon ein Hilfsprogramm haben, mit dem wir die Speisekarte von der Webseite der Pizzeria scrapen.
  * Beispiel einer Speisekarte: `speisekarte.json`
  * Es gibt nur eine Speisekarte, die regelmäßig überschrieben wird.
* Die Speisekarte soll im UI dargestellt werden, damit die Mitarbeiter:innen die Bestellungen für den Tag eingeben können.
  * Berücksichtige die `corporate-design-guidelines` und `frontend-design` Skills
  * Es muss eine Suchfunktion geben, mit der mit "contains" in den Namen der Speisen und Getränke gesucht werden kann
* Eine Bestellung besteht aus:
  * Name des/der Mitarbeiter:in (Freitext, keine gesonderte Mitarbeiter:innenverwaltung)
  * Ausgewählten Speisen und Getränke (mit Mengenangaben)
  * Optionalen Anmerkungen (z.B. "Pizze ohne Zwiebeln"; eine Anmerkung für gesamte Bestellung)
* Die Bestellung bezieht sich immer auf den aktuellen Kalendertag und kann nur bis 10:00 Uhr (lokale Zeit) eingegeben werden. Danach erscheint nur eine Fehlermeldung.
  * Bei Fehlermeldung kann der Benutzer die Speisekarte durchsehen, aber keine Bestellung eingeben.
  * Ab 00:00 Uhr des nächsten Tages können dann wieder Bestellungen für diesen Tag eingegeben werden und es wird keine Fehlermeldung mehr angezeigt.
  * Die Entscheidung über die Uhrzeit muss serverseitig getroffen werden, damit sie nicht durch Manipulation der Client-Uhrzeit umgangen werden kann.
* Bestellungen müssen im Dateisystem in JSON-Dateien (eine pro Bestellung) gespeichert werden (ein Ordner pro Kalendertag)
