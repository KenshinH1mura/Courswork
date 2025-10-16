using System;
using System.Linq;
using System.Windows;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace StreamingService
{
    public partial class MainWindow : Window
    {
        private StreamingContext db;
        private User currentUser;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                db = new StreamingContext();

                if (!db.Database.Exists())
                {
                    MessageBox.Show("Не удалось подключиться к базе данных. Проверьте настройки подключения.", "Ошибка подключения",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoadInitialData();
                CreateTestUser();

                MessageBox.Show("Подключение к базе данных установлено успешно!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка подключения к MySQL: {ex.Message}\n\nПроверьте:\n- Запущен ли MySQL сервер\n- Правильность порта (3308)\n- Правильность логина и пароля",
                    "Ошибка подключения к БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadInitialData()
        {
            db.Genres.Load();
            cmbGenres.ItemsSource = db.Genres.Local;
            cmbGenres.SelectedIndex = -1;

            db.SubscriptionPlans.Load();
            cmbSubscriptionPlans.ItemsSource = db.SubscriptionPlans.Local;
            lvAvailablePlans.ItemsSource = db.SubscriptionPlans.Local;

            db.Media.Include(m => m.Genre).Load();
            lvMedia.ItemsSource = db.Media.Local;
        }

        private void CreateTestUser()
        {
            try
            {
                currentUser = db.Users.FirstOrDefault();
                if (currentUser == null)
                {
                    currentUser = new User
                    {
                        name = "Тестовый пользователь",
                        email = "test@example.com",
                        age = 25,
                        country = "Россия",
                        registration_date = DateTime.Now
                    };
                    db.Users.Add(currentUser);
                    db.SaveChanges();
                }

                UpdateUserInterface();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания пользователя: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUserInterface()
        {
            try
            {
                txtName.Text = currentUser.name;
                txtEmail.Text = currentUser.email;
                txtAge.Text = currentUser.age?.ToString() ?? "";
                txtCountry.Text = currentUser.country ?? "";

                var payments = db.Payments
                    .Include(p => p.Subscription.SubscriptionPlan)
                    .Include(p => p.PaymentMethod)
                    .Include(p => p.Subscription)
                    .Where(p => p.user_id == currentUser.user_id)
                    .OrderByDescending(p => p.payment_date)
                    .ToList();
                lvPayments.ItemsSource = payments;

                var activeSubscription = db.Subscriptions
                    .Include(s => s.SubscriptionPlan)
                    .FirstOrDefault(s => s.user_id == currentUser.user_id && s.status == "Active");

                if (activeSubscription != null)
                {
                    tbCurrentSubscription.Text = $"Текущая подписка: {activeSubscription.SubscriptionPlan.name}";
                    tbSubscriptionStatus.Text = $"Статус: Активна";
                    tbSubscriptionDate.Text = $"Дата активации: {activeSubscription.start_date:dd.MM.yyyy}";
                }
                else
                {
                    tbCurrentSubscription.Text = "Текущая подписка: Отсутствует";
                    tbSubscriptionStatus.Text = "Статус: Неактивна";
                    tbSubscriptionDate.Text = "";
                }

                var viewHistory = db.Views
                    .Include(v => v.Media.Genre)
                    .Where(v => v.user_id == currentUser.user_id)
                    .OrderByDescending(v => v.view_date)
                    .ToList();
                lvViewHistory.ItemsSource = viewHistory;

                var totalViews = viewHistory.Count;
                var totalWatchTime = viewHistory.Sum(v => v.watch_time);
                var uniqueMedia = viewHistory.Select(v => v.media_id).Distinct().Count();

                tbStats.Text = $"Всего просмотров: {totalViews} | Уникального контента: {uniqueMedia} | Общее время: {totalWatchTime} мин";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления интерфейса: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                currentUser.name = txtName.Text;
                currentUser.email = txtEmail.Text;
                currentUser.age = int.TryParse(txtAge.Text, out int age) ? age : (int?)null;
                currentUser.country = txtCountry.Text;

                db.SaveChanges();
                MessageBox.Show("Изменения профиля сохранены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения профиля: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BuySubscription_Click(object sender, RoutedEventArgs e)
        {
            var selectedPlan = cmbSubscriptionPlans.SelectedItem as SubscriptionPlan;
            if (selectedPlan == null)
            {
                MessageBox.Show("Выберите тарифный план для активации", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var oldSubscriptions = db.Subscriptions.Where(s => s.user_id == currentUser.user_id);
                foreach (var sub in oldSubscriptions)
                {
                    sub.status = "Inactive";
                }

                var newSubscription = new Subscription
                {
                    user_id = currentUser.user_id,
                    plan_id = selectedPlan.plan_id,
                    status = "Active",
                    start_date = DateTime.Now
                };
                db.Subscriptions.Add(newSubscription);
                db.SaveChanges();

                var paymentMethod = db.PaymentMethods.First();
                var payment = new Payment
                {
                    user_id = currentUser.user_id,
                    subscription_id = newSubscription.subscription_id,
                    method_id = paymentMethod.method_id,
                    amount = selectedPlan.monthly_price,
                    payment_date = DateTime.Now
                };
                db.Payments.Add(payment);

                db.SaveChanges();

                MessageBox.Show($"Подписка '{selectedPlan.name}' успешно активирована!\nСписано: {selectedPlan.monthly_price}₽",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                UpdateUserInterface();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при активации подписки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PauseSubscription_Click(object sender, RoutedEventArgs e)
        {
            var activeSubscription = db.Subscriptions
                .FirstOrDefault(s => s.user_id == currentUser.user_id && s.status == "Active");

            if (activeSubscription != null)
            {
                activeSubscription.status = "Paused";
                db.SaveChanges();
                MessageBox.Show("Подписка приостановлена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateUserInterface();
            }
            else
            {
                MessageBox.Show("Активная подписка не найдена", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void WatchMedia_Click(object sender, RoutedEventArgs e)
        {
            var selectedMedia = lvMedia.SelectedItem as Media;
            if (selectedMedia == null)
            {
                MessageBox.Show("Выберите контент для просмотра", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var hasActiveSubscription = db.Subscriptions
                .Any(s => s.user_id == currentUser.user_id && s.status == "Active");

            if (!hasActiveSubscription)
            {
                MessageBox.Show("Для просмотра контента необходима активная подписка", "Ошибка доступа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var random = new Random();
                var watchTime = random.Next(30, 121);

                var view = new View
                {
                    user_id = currentUser.user_id,
                    media_id = selectedMedia.media_id,
                    view_date = DateTime.Now,
                    watch_time = watchTime,
                    device = "Desktop App"
                };
                db.Views.Add(view);
                db.SaveChanges();

                MessageBox.Show($"Вы посмотрели: {selectedMedia.title}\nВремя просмотра: {watchTime} минут",
                    "Просмотр завершен", MessageBoxButton.OK, MessageBoxImage.Information);

                UpdateUserInterface();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи просмотра: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenreFilter_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedGenre = cmbGenres.SelectedItem as Genre;
            if (selectedGenre != null)
            {
                lvMedia.ItemsSource = db.Media.Local
                    .Where(m => m.genre_id == selectedGenre.genre_id)
                    .ToList();
            }
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            cmbGenres.SelectedIndex = -1;
            lvMedia.ItemsSource = db.Media.Local;
        }

        private void Media_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }

        protected override void OnClosed(EventArgs e)
        {
            db?.Dispose();
            base.OnClosed(e);
        }
    }
}