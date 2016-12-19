using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Parser_Test
{
    //struct to hold info on each lines
    public struct node
    {
        //public Rect w;
        public string line;
        public bool response;
        public string owner;
        public List<gate> flags;
        public int sceneNum;
        public int lineNum;
        public int jumpNum;

    }
    public struct gate
    {
        public bool f;
        public int effect;
        public string name;
    }
    //holds a bunch of windows which represent nodes each of which is a line of dialogue
    //all nodes are grouped into levels
    public class tree
    {

        public tree()
        {
            title = "undefined";
            actors = new List<string>();
            gates = new List<gate>();
            nodes = new List<node>();
        }

        public string title;
        public List<string> actors;
        public List<gate> gates;
        //keep it one dimensional for initial testing
        //public List<List<node>> nodes;
        public List<node> nodes;
    }

    class Program
    {
        static void Main(string[] args)
        {
            string tmp = File.ReadAllText("script.txt");
            parse(tmp);
        }

        //create a dialogue tree based on  text script
        static tree parse(string f)
        {

            //each set of lines is divided by ~
            //string[] sections = f.Split('~');
            string[] lines;
            string[] segments;
            string characters;

            //counting all the lines without data
            int offset = 0;
            int curScene = 0;

            //tree to return
            tree result = new tree();

            //the first set will always be the name of the conversation and some variable dec

            //split on ~
            string[] scenes = f.Split('~');

            for (int i = 0; i < scenes.Length; i++)
            {

                //split the section into individual lines
                lines = scenes[i].Split('\n', '\r');

                //reset the counter
                offset = 0;

                for (int j = 0; j < lines.Length; j++)
                {
                    

                    if (lines[j] == "")
                    {
                        //offset++;
                        continue;
                    }
                    offset++;
                    //split the line into segments based on tabs
                    segments = lines[j].Split('\t');

                    for (int k = 0; k < segments.Length; k++)
                    {
                        characters = segments[k];

                        int end;
                        string tmp;

                        //checks the first part of every segment for a tag 
                        if (characters != "")
                        {
                            switch (characters[0])
                            {
                                case '/': // lines that start with // are notes and should be ignored
                                    //decrement offset to keep stable
                                    offset--;
                                    continue;


                                case '{': // find the scene num
                                    end = characters.LastIndexOf('}');
                                    tmp = characters.Substring(1, end - 1);
                                    curScene = int.Parse(tmp);
                                    //this is the only tag that will exist after the variable dec
                                    //dec offset to keep in sync
                                    offset--;
                                    break;

                                case '#': // lines starting text inside  a set of # means the title for the script

                                    result.title = segments[k];
                                    continue;

                                case '|': // | is used to declare script participants

                                    end = characters.LastIndexOf('|');

                                    tmp = characters.Substring(1, end - 1);
                                    result.actors.Add(tmp);
                                    break;

                                case '<': // <> used to declare script variables
                                    gate generated = new gate();
                                    end = characters.LastIndexOf('>');
                                    tmp = characters.Substring(1, end - 1);
                                    generated.name = tmp;

                                    result.gates.Add(generated);
                                    continue;

                                case '[': // used to declare actions that should happen if this line is selected
                                    end = characters.LastIndexOf(']');
                                    tmp = characters.Substring(1, end - 1);
                                    continue;

                                default: // if none of these cases that means that no declarations are present
                                    break;
                            }

                            //after checking for tags we then parse
                            //loop through all available conversation participants to see who  owns the line
                            if (result.actors.Contains(characters))
                            {
                                // the next segment will be the line of dialogue and the one after that will be the command to be executed
                                node created = new node();
                                end = characters.Length;
                                created.owner = characters.Substring(0, end);
                                created.lineNum = offset;
                                end = segments[k + 1].LastIndexOf('\"');
                                created.line = segments[k + 1].Substring(1, end - 1);
                                created.sceneNum = curScene;
                                //check to make sure that there are commands since not all lines will have them
                                if (segments.Length >= k + 2)
                                {
                                    //node that this node links to
                                    //grab the jump number from inside the segment
                                    string t = segments[k + 2];
                                    int p = t.LastIndexOf(']');
                                    t = t.Substring(1, p - 1);
                                    created.jumpNum = int.Parse(t);
                                }
                                result.nodes.Add(created);

                            }


                        }
                    }
                }
            }





            return result;
        }
    }
}
