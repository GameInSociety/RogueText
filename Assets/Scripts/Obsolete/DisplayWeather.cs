using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayWeather : MonoBehaviour {

    public static DisplayWeather Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateDescription()
    {
		if (TimeManager.GetInstance().changedPartOfDay == false)
        {
            return;
        }

        DisplayCurrentWeather();
		
	}

    public void DisplayCurrentWeather()
    {
        string str = "";

        if (TimeManager.GetInstance().raining)
        {
            if (Interior.GetCurrent != null)
            {
                switch (TimeManager.GetInstance().currentPartOfDay)
                {
                    case TimeManager.PartOfDay.Dawn:
                        str = "Une faible lueur, derrière la pluie, rentre dans la pièce";
                        break;
                    case TimeManager.PartOfDay.Morning:
                        str = "Malgré le ciel pluvieux, la pièce commence légerement à s'éclairer";
                        break;
                    case TimeManager.PartOfDay.Noon:
                        str = "La pluie tombe d'un ciel blanc, qui éclaire la pièce.";
                        break;
                    case TimeManager.PartOfDay.Afternoon:
                        str = "La pluie bat à son pluie fort sur le toit de la maison";
                        break;
                    case TimeManager.PartOfDay.Dusk:
                        str = "Le ciel gris se noirci lentement";
                        break;
                    case TimeManager.PartOfDay.Night:
                        str = "La maison est plongée dans la nuit et le noir. On entend toujours la pluie tomber, dehors";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (TimeManager.GetInstance().currentPartOfDay)
                {
                    case TimeManager.PartOfDay.Dawn:
                        str = "Une faible lueur commence à apparaitre derrière la pluie battante";
                        break;
                    case TimeManager.PartOfDay.Morning:
                        str = "Le ciel est blanc, et vous commencez à voir clair aux malgré la pluie.";
                        break;
                    case TimeManager.PartOfDay.Noon:
                        str = "Le soleil brille derrière le ciel gris et la pluie";
                        break;
                    case TimeManager.PartOfDay.Afternoon:
                        str = "La pluie de l'après midi est chaude et lourde";
                        break;
                    case TimeManager.PartOfDay.Dusk:
                        str = "La lumière qui traversait le ciel disparait doucement. Le crépuscule s'installe.";
                        break;
                    case TimeManager.PartOfDay.Night:
                        str = "La nuit est tombée, la pluie tombe dans la pénombre la plus totale...";
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            if (Interior.GetCurrent != null)
            {
                switch (TimeManager.GetInstance().currentPartOfDay)
                {
                    case TimeManager.PartOfDay.Dawn:
                        str = "La lumière de l'aube commence à rentrer dans la pièce";
                        break;
                    case TimeManager.PartOfDay.Morning:
                        str = "La pièce est éclairée par le soleil du matin";
                        break;
                    case TimeManager.PartOfDay.Noon:
                        str = "";
                        break;
                    case TimeManager.PartOfDay.Afternoon:
                        str = "L'air de l'après midi est chaud est dense dans la maison";
                        break;
                    case TimeManager.PartOfDay.Dusk:
                        str = "Le lumière du soleil disparait doucement et la maison s'assombrit";
                        break;
                    case TimeManager.PartOfDay.Night:
                        str = "La nuit est tombée, toute la maison est plongée dans la pénombre...";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (TimeManager.GetInstance().currentPartOfDay)
                {
                    case TimeManager.PartOfDay.Dawn:
                        str = "Le soleil apparait doucement et le ciel s'éclaircie";
                        break;
                    case TimeManager.PartOfDay.Morning:
                        str = "Il est encore tôt dans la journée, le matin s'est installé.";
                        break;
                    case TimeManager.PartOfDay.Noon:
                        str = "Le soleil est au dessus de vous, ce doit être le milieu de journée";
                        break;
                    case TimeManager.PartOfDay.Afternoon:
                        str = "L'air de l'après midi est chaud est dense";
                        break;
                    case TimeManager.PartOfDay.Dusk:
                        str = "Le soleil disparait doucement. Le crépuscule s'installe.";
                        break;
                    case TimeManager.PartOfDay.Night:
                        str = "La nuit est tombée, tout est noir autour de vous...";
                        break;
                    default:
                        break;
                }
            }
        }

        //Display(str);
    }
}
