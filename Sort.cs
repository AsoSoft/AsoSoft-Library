using System;
using System.Collections.Generic;
using System.Text;

namespace AsoSoftLibrary
{
    public static partial class AsoSoft
    {
	   /// <summary>Sorting a string list in correct order of Kurdish alphabet.</summary>
	   public static List<string> KurdishSort(List<string> inputList)
	   {
		  var ku = new List<char>();
		  ku.AddRange("ئءاآأإبپتثجچحخدڎڊذرڕزژسشصضطظعغفڤقكکگڴلڵمنوۆۊۉۋهھەیێ");
		  return CustomSort(inputList, ku);
	   }
	   /// <summary>Sorting a string list in custom order.</summary>
	   public static List<string> CustomSort(List<string> inputList, List<char> inputOrder)
	   {
		  var baseChar = 62000;//  9472;
		  var order = new List<char>();
		  for (int i = 0; i < inputOrder.Count; i++)
			 order.Add((char)(baseChar + i));
		  for (int i = 0; i < inputList.Count; i++)
			 for (int j = 0; j < order.Count; j++)
				inputList[i] = inputList[i].Replace(inputOrder[j], order[j]);
		  inputList.Sort();
		  for (int i = 0; i < inputList.Count; i++)
			 for (int j = 0; j < order.Count; j++)
				inputList[i] = inputList[i].Replace(order[j], inputOrder[j]);
		  return inputList;
	   }
    }
}
