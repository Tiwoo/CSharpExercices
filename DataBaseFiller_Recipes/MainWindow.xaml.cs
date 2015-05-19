using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Data.SqlClient;
using Microsoft.Win32;
using System.IO;
using System.Data;


namespace DataBaseFiller_Recipes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection conn = new SqlConnection("Server=MININT-NGAKU1U\\SQLEXPRESS;Database=Recipes_Test1;Trusted_Connection=True;");
        SqlCommand command;
        string imageLoc = "";
        
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void recipeImageUploadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog imageUploadDialogBox = new OpenFileDialog();
                imageUploadDialogBox.DefaultExt = ".jpg";
                imageUploadDialogBox.Filter = "JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif|All Files (*.*)|*.*";
                imageUploadDialogBox.Title = "Recipe Image";
                if (imageUploadDialogBox.ShowDialog() == true)
                {
                    imageLoc = imageUploadDialogBox.FileName.ToString();
                    recipeImage.Source = new BitmapImage(new Uri(imageLoc));
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void saveRecipeInDataBaseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] img = null;
                FileStream myFileStream = new FileStream(imageLoc,FileMode.Open, FileAccess.Read);
                BinaryReader myBinaryReader = new BinaryReader(myFileStream);
                img = myBinaryReader.ReadBytes((int)myFileStream.Length);            
                TextRange recipeIngredientTextRange = new TextRange(recipeIngredientsRichTextBox.Document.ContentStart, recipeIngredientsRichTextBox.Document.ContentEnd);       
                TextRange recipeSummaryTextRange = new TextRange(recipeSummaryRichTextBox.Document.ContentStart, recipeSummaryRichTextBox.Document.ContentEnd);
                TextRange recipeCookingInstructionsTextRange = new TextRange(recipeCookingInstructionsRichTextBox.Document.ContentStart, recipeCookingInstructionsRichTextBox.Document.ContentEnd);
                string sqlRowsCount = "SELECT COUNT(*) FROM dbo.Recipes3";
                int rowsCount = 0;
                using (SqlCommand cmdRowsCount = new SqlCommand(sqlRowsCount, conn))
                {
                    conn.Open();
                    rowsCount = (int)cmdRowsCount.ExecuteScalar() + 1;
                    conn.Close();
                }
                
                //string sqlSave = "INSERT INTO Recipes3(RECIPE_ID, RECIPE_NAME, RECIPE_TYPE, RECIPE_DURATION, RECIPE_INGREDIENTS, RECIPE_SUMMARY, RECIPE_COOKING_INSTRUCTIONS, RECIPE_IMAGE) VALUES(" + rowsCount + ",'" + recipeNameTextBox.Text + "','" + recipeTypeTextBox.Text + "','" + recipeDurationTextBox.Text + "','" + recipeIngredientTextRange.Text + "','" + recipeSummaryTextRange.Text + "','"+ recipeCookingInstructionsTextRange.Text+"',@img)";
                // the statement above used string concatenation to build the SQL statement. not a good practise. It is better to use SQL parameters as below
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    string sqlSave = "INSERT INTO Recipes3(RECIPE_ID, RECIPE_NAME, RECIPE_TYPE, RECIPE_ORIGIN, RECIPE_DURATION, RECIPE_INGREDIENTS, RECIPE_SUMMARY, RECIPE_COOKING_INSTRUCTIONS, RECIPE_IMAGE) VALUES (@ID, @Name, @Type, @Origin, @Duration, @Ingredients, @Summary, @Cooking, @Image)";
                    command = new SqlCommand(sqlSave, conn);
                    command.Parameters.AddWithValue("@ID", rowsCount);
                    command.Parameters.AddWithValue("@Name", recipeNameTextBox.Text);
                    command.Parameters.AddWithValue("@Type", recipeTypeTextBox.Text);
                    command.Parameters.AddWithValue("@Origin", recipeOriginTextBox.Text);
                    command.Parameters.AddWithValue("@Duration", recipeDurationTextBox.Text);
                    command.Parameters.AddWithValue("@Ingredients", recipeIngredientTextRange.Text);
                    command.Parameters.AddWithValue("@Summary", recipeSummaryTextRange.Text);
                    command.Parameters.AddWithValue("@Cooking", recipeCookingInstructionsTextRange.Text);
                    command.Parameters.AddWithValue("@Image", img);
                    //command.Parameters.Add(new SqlParameter("@imgage", img));
                    int x = command.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show(x.ToString() + " record(s) saved.");
                    recipeIDTextBox.Text = "";
                    recipeImage.Source = null;
                    recipeNameTextBox.Text = "";
                    recipeDurationTextBox.Text = "";
                    recipeOriginTextBox.Text = "";
                    recipeTypeTextBox.Text = "";
                    recipeIngredientTextRange.Text = "";
                    recipeSummaryTextRange.Text = "";
                    recipeCookingInstructionsTextRange.Text = "";
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show(ex.Message);
            }
        }

        private void showRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sqlShow = "SELECT RECIPE_NAME, RECIPE_TYPE, RECIPE_ORIGIN, RECIPE_DURATION, RECIPE_INGREDIENTS, RECIPE_SUMMARY, RECIPE_COOKING_INSTRUCTIONS, RECIPE_IMAGE FROM Recipes3 WHERE RECIPE_ID = " + recipeIDTextBox.Text;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    command = new SqlCommand(sqlShow, conn);
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Read();
                    if (reader.HasRows)
                    {
                        recipeNameTextBox.Text = reader[0].ToString();
                        recipeTypeTextBox.Text = reader[1].ToString();
                        recipeOriginTextBox.Text = reader[2].ToString();
                        recipeDurationTextBox.Text = reader[3].ToString();
                        recipeIngredientsRichTextBox.AppendText(reader[4].ToString());
                        recipeSummaryRichTextBox.AppendText(reader[5].ToString());
                        recipeCookingInstructionsRichTextBox.AppendText(reader[6].ToString());
                        byte[] img = (byte[])(reader[7]);
                        if (img == null)
                        {
                            recipeImage = null;
                        }
                        else
                        {
                                Stream ms = new MemoryStream(img);
                            
                                BitmapImage image = new BitmapImage();
                                image.BeginInit();
                                image.StreamSource = ms;
                                image.EndInit();
                                recipeImage.Source = image;
                            
                        }

                    }
                    else
                    {
                        MessageBox.Show("This ID does not exist");
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show(ex.Message);
            }
        }

        private void deleteRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    using (SqlCommand deleteRecipe = new SqlCommand("DELETE FROM Recipes3 WHERE RECIPE_ID = " + recipeIDTextBox.Text + "", conn))
                    {
                        int numberOfDeletedRow = deleteRecipe.ExecuteNonQuery();
                        MessageBox.Show(numberOfDeletedRow.ToString() + " row has been deleted from the table");
                        recipeIDTextBox.Text = "";
                        recipeImage.Source = null;
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                MessageBox.Show(String.Format("An error has occured:{0}", ex.Message));
            }
        }
    }
}
