using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class SetMatch : MonoBehaviour
{
    [SerializeField] private InputField nameInput, homeGoal, awayGoal;
    [SerializeField] private Text textHome, textAway, warning, textSummary;
    [SerializeField] private GameObject sendButton, panelLoad, panelBet, panelSummary, imageGO;

    Animation anim, animImage;
    private int team = 0;
    private string[] teams;
    private int[] scores;
    private string fileName;

    private string url = "https://gist.githubusercontent.com/qbic7/f237e5a179c9d0aa6e2f2dd2960c4435/raw";

    public void loadMatches()
    {
        StartCoroutine(GetTextFromWWW());

        fileName = nameInput.text + ".txt";

        panelLoad.SetActive(false);
        panelBet.SetActive(true);
    }

    public void playAnims()
    {
        animImage.Play();
        anim.Play();
    }

    public void saveScoreHome()
    {
        try
        {
            scores[team] = Convert.ToInt32(homeGoal.text);
        }
        catch (FormatException)
        {
            warning.text = "Wynik musi być liczbą!";
            playAnims();
        }
    }

    public void saveScoreAway()
    {
        try
        {
            scores[team + 1] = Convert.ToInt32(awayGoal.text);
        }
        catch (FormatException)
        {
            warning.text = "Wynik musi być liczbą!";
            playAnims();
        }
    }

    //podsumowanie typów
    public void scoresDone()
    {
        bool done = false;

        for (int i = 0; i < teams.Length; i += 2)
        {
            if (scores[i].Equals(-946) || scores[i + 1].Equals(-946))
            {
                warning.text = "Uzupełnij wynik meczu " + teams[i] + " - " + teams[i + 1];
                playAnims();
                done = false;
                break;
            }
            else
            {
                done = true;
            }
        }

        if (done)
        {
            summaryBets();
            panelBet.SetActive(false);
            panelSummary.SetActive(true);
        }
    }

    public void editBets()
    {
        panelSummary.SetActive(false);
        panelBet.SetActive(true);
    }

    //zapis do pliku
    public void summaryDone()
    {
        panelSummary.SetActive(false);
        sendButton.SetActive(true);

        using (StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/" + fileName))
        {
            for (int i = 0; i < teams.Length; i++)
            {
                writer.WriteLine(scores[i]);
            }
        }
    }

    public void summaryBets()
    {
        textSummary.text = buildString();
    }

    //Budowanie stringa do podsumowania
    public string buildString()
    {
        StringBuilder sb = new StringBuilder(1200);

        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i] != null)
            {
                sb.Append(scores[i] + " " + teams[i] + "\n");

                if (i % 2 == 1)
                {
                    sb.Append(" \n");
                }
            }
        }

        return sb.ToString();
    }

    public void sendEmail()
    {
        MailMessage mail = new MailMessage();

        mail.From = new MailAddress("typerligalks@gmail.com");
        mail.To.Add("typerligalks@gmail.com");
        mail.Subject = "LigaTyper";
        mail.Body = "Wysyłam moje typy!";

        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new NetworkCredential("typerligalks@gmail.com", "lksdabdebnica7") as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
        mail.Attachments.Add(new Attachment(Application.persistentDataPath + "/" + fileName));
        try
        {
            smtpServer.Send(mail);
            warning.text = "Wysłano wiadomość!";
            playAnims();
        }
        catch
        {
            warning.text = "Nie mozna wysłać e-mail, spróbuj ponownie!";
            playAnims();
        }
    }

    //wyświetlanie wcześniejszych typów w okienkach Input
    public void displayInput()
    {
        if (scores[team].Equals(-946))
        {
            homeGoal.GetComponentInChildren<Text>().text = null;
        }
        else
        {
            homeGoal.GetComponentInChildren<Text>().text = Convert.ToString(scores[team]);
        }

        if (scores[team + 1].Equals(-946))
        {
            awayGoal.GetComponentInChildren<Text>().text = null;
        }
        else
        {
            awayGoal.GetComponentInChildren<Text>().text = Convert.ToString(scores[team + 1]);
        }
    }

    public void nextMatch()
    {
        if (team < teams.Length - 2)
        {
            team += 2;
        }

        setTeamsText();
    }

    public void previousMatch()
    {
        if (team > 1)
        {
            team -= 2;
        }

        setTeamsText();
    }

    public void setTeamsText()
    {
        textHome.text = teams[team];
        textAway.text = teams[team + 1];
        displayInput();
    }

    public void tabInit(int numberOfTeams)
    {
        teams = new string[numberOfTeams];
        scores = new int[numberOfTeams];

        for (int i = 0; i < teams.Length; i++)
        {
            scores[i] = -946;
        }
    }

    IEnumerator GetTextFromWWW()
    {
        WWW wWebsite = new WWW(url);

        string[] tab = new string[61];

        yield return wWebsite;

        if (wWebsite.error != null)
        {
            warning.text = "Nie można otworzyć strony!";
            playAnims();
        }
        else
        {
            tab = wWebsite.text.Split('\n');

            tabInit(Convert.ToInt32(tab[0]));

            for (int i = 1; i < tab.Length; i++)
            {
                if (tab[i] != null)
                {
                    teams[i - 1] = tab[i];
                }
            }

            textHome.text = teams[team];
            textAway.text = teams[team + 1];

            warning.text = "Załadowano dane!";
            playAnims();
        }
    }

    // Use this for initialization
    void Start()
    {
        anim = warning.GetComponent<Animation>();
        animImage = imageGO.GetComponent<Animation>();
    }
}
