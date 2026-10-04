# מדריך כניסה לעבודה על Chinese Sale

ברוך/ה הבא/ה לפרויקט. המסמך הזה נועד לאפשר כניסה רגועה, מסודרת ועצמאית לעבודה על **Chinese Sale MVP**.

## 1. היכרות עם הפרויקט

Chinese Sale הוא אתר למכירת כרטיסים למכירה סינית: משתמשים יכולים להירשם ולהתחבר, לצפות במתנות ובקטגוריות, לרכוש כרטיסים, ובשלבים המתאימים להשתתף בהגרלה. במערכת קיימים גם תפקידי מנהל ותורם ותהליכי ניהול, תשלום פיתוח וזוכים.

הארכיטקטורה מחולקת לשני חלקים:

- **Backend**: Web API של ASP.NET Core על .NET 8, עם Controllers, שכבות Bll ו-DAL, DTOs, AutoMapper, JWT ו-Entity Framework Core.
- **Frontend**: אפליקציית Angular 20, עם routes, components, services, מודלים ו-UI המבוסס גם על Angular Material ו-Bootstrap.
- **תקשורת**: ה-Frontend פונה ל-API באמצעות `HttpClient`. כתובת ה-API המוגדרת בשירות ה-HTTP היא `https://localhost:7142/api/`.

## 2. הכנת סביבת העבודה

יש להתקין מראש:

- Git.
- .NET 8 SDK.
- Node.js בגרסה התואמת ל-Angular 20, כולל `npm`.
- סביבת פיתוח כמו VS Code או Visual Studio.
- SQL Server או סביבת מסד הנתונים שהוגדרה לצוות, אם נדרשת גישה לפעולות הנשענות על נתונים.

לאחר השכפול, פותחים שני טרמינלים נפרדים: אחד ל-Backend ואחד ל-Frontend. כך אפשר להריץ את שני החלקים במקביל.

## 3. Git: כניסה ראשונה ופתיחת ענף אישי

יש לעבוד בענף אישי ולא ישירות על `Dev`.

```powershell
git clone <REPOSITORY_URL>
Set-Location ChineseSale
git switch -c Gilli
git push -u origin Gilli
```

אם הענף `Gilli` כבר קיים בשרת:

```powershell
git fetch origin
git switch Gilli
git pull --ff-only origin Gilli
```

לפני כל עבודה כדאי לוודא את המצב:

```powershell
git status --short --branch
git branch -vv
```

### שגרת עבודה מומלצת

1. בתחילת כל יום מקבלים את העדכונים של `Dev` אל הענף האישי:

   ```powershell
   git fetch origin
   git pull --ff-only origin Dev
   ```

   אם יש שינויים מקומיים לא שמורים או שהפקודה אינה יכולה לבצע Fast-forward, עוצרים ומתאמים לפני שממשיכים. אין לבצע `reset`, `clean`, `checkout` או מחיקה של עבודה קיימת בלי אישור מפורש.

2. עובדים, בודקים ומבצעים commits קטנים וברורים בענף האישי.
3. לפני שיתוף העבודה מריצים את הבדיקות הרלוונטיות ובודקים את ה-diff.
4. דוחפים את הענף:

   ```powershell
   git push origin Gilli
   ```

5. בסיום משימה פותחים Pull Request מ-`Gilli` אל `Dev`, עם תיאור קצר של השינוי, בדיקות שבוצעו ומגבלות ידועות.

לפעולות Git משתמשים ב-agent בשם `git` וב-skill בשם `git-workflow`. הם מסייעים לבדוק ענפים, upstream, שינויים, commits ו-PRs תוך שמירה על עבודה קיימת. לא מבצעים commit, push או PR אוטומטית בלי לוודא שהיקף הפעולה ברור.

## 4. איך עובדים עם Agents ו-Skills

התיקייה `.github` מכילה את הוראות העבודה של הפרויקט:

- `.github/agents/` מכילה agents ייעודיים: `contract-baseline`, `auth`, `catalog`, `purchase-payment`, `admin`, `lottery`, `chinese-sale-phase` ו-`git`.
- `.github/skills/` מכילה את הידע וההליך הנדרש לכל תחום, כולל `contract-baseline`, `auth`, `catalog`, `purchase-payment`, `admin`, `lottery` ו-`git-workflow`.
- `.github/instructions/` מכילה כללי בטיחות ועבודה משותפים לקבצי C#, Angular ותיעוד.

Agent הוא מומחה לתחום עבודה; Skill הוא ההליך והכללים שה-agent צריך להפעיל. לפני שמתחילים משימה בוחרים את ה-agent וה-skill המתאימים, קוראים את `plan.md`, ומוודאים שהעבודה נמצאת בשלב הנכון.

### Gated Sub-plan: שער האישור

כל שלב משמעותי עובד בתהליך אישור מדורג:

1. קוראים את `plan.md`, את הוראות ה-agent וה-skill ואת הקוד שבאחריות המודול.
2. מנתחים חוזים, תלויות, סיכונים, קבצים וסמלים שישתנו.
3. מציגים **Gated Sub-plan** מפורט: גבולות השינוי, קבצים מדויקים, קריטריוני בדיקה ותלויות.
4. עוצרים וממתינים לאישור מפורש.
5. רק לאחר האישור עורכים קוד, מריצים בדיקות ומעדכנים את `plan.md` עם החלטות, תוצאות ו-handoffs.

במיוחד אין לשנות את `backend/ApiProject/Project/Models/` או את `Migrations/`, ואין להריץ פקודות migration או עדכון מסד נתונים, ללא אישור מפורש לשינוי המסוים. הרשאות Backend הן מקור האמת; Guards ב-Angular משפרים ניווט אך אינם אבטחה.

## 5. חלוקת אחריות לעבודה במקביל

כדי לצמצם Merge Conflicts, כל אחד עובד בעיקר בתוך גבול מודול ברור. לפני שינוי בקובץ משותף מתאמים עם השותף ומעדכנים את ה-Sub-plan.

### חוזה ותשתית Backend

אחראי על בדיקת המודלים הקיימים, DTOs, Profiles, Controllers, Bll ו-DAL, ועל תיעוד חוזי ה-API. זהו שלב התשתית (`contract-baseline`) והוא צריך להסתיים לפני התאמות Angular. אין להוסיף ישויות או לשנות סכימה בלי אישור נפרד.

### Auth

אחראי על `UserController`, `UserService`, JWT, claims, interceptor, logout, expiry, auth guard ו-admin guard. כולל הרשמה, התחברות והפרדה בין `User`, `Donor` ו-`Admin` לפי החוזה שאושר.

### Catalog

אחראי על קטלוג ציבורי: מתנות, קטגוריות, חיפוש, סינון, רשימות ופרטי מתנה. בצד Angular: services, models, routes ו-components הקשורים ל-home, presents ו-categories, כולל מצבי loading, empty ו-error.

### Purchase ו-Payment

אחראי על כרטיסים, בעלות, זמינות, checkout, אזור אישי ותשלום פיתוח. הבעלות ומצב התשלום נקבעים בשרת; אין לסמוך על `userId`, `CreatedBy` או `IsPaid` שהלקוח שולח.

### Admin ו-Donor

אחראי על מסכי ניהול למתנות, קטגוריות, תורמים, כרטיסים ורכישות, כולל הרשאות Admin, ולידציה, active/deleted וטיפול בשגיאות שרת. תורם מיוצג במערכת כ-`User` בעל role מתאים, ולא כישות חדשה ללא אישור.

### Lottery ו-Winner

אחראי על הגרלה, זכאות של כרטיסים ששולמו, שמירת זוכים, idempotency והרשאות. מסכי הזוכים והודעות נבנים רק לפי נתונים שמגיעים מה-API, לא לפי נתוני דמה ב-Frontend.

### קבצים משותפים

`plan.md`, routes ראשיים, `app.config.ts`, שירות HTTP, מודלים משותפים וקבצי styles עלולים לגרום להתנגשויות. משנים אותם רק לאחר תיאום, ב-commit ממוקד, ומעדכנים את השותף מיד.

## 6. צעדים ראשונים להרצה מקומית

### Backend

```powershell
Set-Location backend\ApiProject\Project
dotnet restore
dotnet build
dotnet run --launch-profile http
```

לאחר ההפעלה פותחים:

- Swagger: `http://localhost:5023/swagger`
- API ב-HTTPS: `https://localhost:7142`
- API ב-HTTP: `http://localhost:5023`

אפשר גם להריץ את הפרויקט מתוך Visual Studio או VS Code לפי פרופיל `http` או `https`. אם מופיעה אזהרת תעודת HTTPS מקומית, פועלים לפי הגדרת סביבת הפיתוח המקומית של .NET.

### Frontend

בטרמינל שני:

```powershell
Set-Location frontend
npm install
ng s -o
```

פותחים `http://localhost:4200/`. בזמן הפיתוח Angular יבנה מחדש לאחר שינויים. בדיקות שימושיות:

```powershell
npm run build
npm test
```

אם ה-Frontend אינו מצליח להתחבר, בודקים קודם שה-API רץ, שהכתובת בשירות ה-HTTP תואמת לפרופיל שנבחר, ושאין חסימת תעודת HTTPS או CORS.

## 7. `plan.md` הוא מקור האמת

`plan.md` הוא מסמך העבודה המרכזי של הפרויקט, מעין "סיסמת כניסה" משותפת להבנת ההקשר לפני כל משימה. הוא מגדיר את סדר השלבים, החלטות שאושרו, מגבלות, תלויות, סטטוס, handoffs ותוצאות בדיקה.

לפני שמתחילים משימה:

1. קוראים את ה-Mission, ה-Status וה-Execution Contract.
2. בודקים באיזה שלב המשימה נמצאת ומה חסום.
3. מאמתים שהקבצים המתוכננים נמצאים בתחום האחריות.
4. לאחר העבודה מוסיפים לוג handoff קצר עם קבצים ששונו, בדיקות, סיכונים ותלות בשלב הבא.

## 8. סדר כניסה מומלץ ביום הראשון

1. לשכפל את המאגר ולפתוח את `Gilli`.
2. לקרוא את `plan.md` ואת המדריך הזה.
3. להריץ `dotnet build` ו-`npm run build` כדי לזהות את מצב הבסיס.
4. להפעיל את ה-API ולוודא ש-Swagger נפתח.
5. להפעיל את Angular ולוודא שהאפליקציה נטענת ב-`localhost:4200`.
6. לבחור מודול אחד, לקרוא את ה-agent וה-skill המתאימים, ולהכין Gated Sub-plan לפני שינוי קוד.
7. לתאם עם השותף את הקבצים המשותפים ואת גבולות המשימה.

כך נשמור על קצב עבודה נעים, היסטוריה ברורה, פחות התנגשויות ויכולת לחבר את כל חלקי ה-MVP בהדרגה.