# FeedbackApp - Survey & Feedback Generator

A beautiful, modern survey/feedback application built with ASP.NET Core Razor Pages. Features a cute, aesthetic UI with pastel colors, smooth animations, and a powerful question builder.

## Features

### Admin Features
- **Create Events** with custom surveys
- **Question Builder** - Add multiple question types:
  - Short Text
  - Long Text (TextArea)
  - Single Choice (Radio buttons)
  - Multiple Choice (Checkboxes)
  - Yes/No Toggle
- **Optional Star Rating** - Enable/disable per event
- **Shareable Links** - Generate unique links for each event
- **Edit Events** - Modify questions anytime
- **View Responses** - See all feedback with question answers
- **Delete Feedback** - Remove individual responses
- **Dashboard** - Overview with statistics

### User Features
- **Link-only Access** - Users can only access via shared link
- **Anonymous Submissions** - Name is optional
- **Interactive Forms** - Star ratings, radio buttons, checkboxes
- **Clean UI** - No distractions, focus on feedback

### Technical Features
- **File-based JSON Storage** - No database required
- **Thread-safe Operations** - Using SemaphoreSlim
- **Session-based Auth** - Simple admin login
- **Responsive Design** - Works on all devices
- **Tailwind CSS** - Modern, beautiful styling

## Project Structure

```
FeedbackApp/
├── Models/
│   ├── Event.cs          # Event with questions, rating toggle
│   ├── Feedback.cs       # Feedback with dynamic answers
│   └── Question.cs       # Question types and options
├── Services/
│   ├── Interfaces/
│   ├── EventService.cs
│   ├── FeedbackService.cs
│   └── JsonStorageService.cs
├── Pages/
│   ├── Index.cshtml            # Landing page
│   ├── Feedback/
│   │   └── Submit.cshtml       # User feedback form (token-based)
│   ├── Admin/
│   │   ├── Index.cshtml        # Dashboard with share links
│   │   ├── Login.cshtml
│   │   └── Events/
│   │       ├── Create.cshtml   # Create with question builder
│   │       ├── Edit.cshtml     # Edit questions
│   │       └── Details.cshtml  # View responses
│   └── Shared/
│       └── _Layout.cshtml
├── Data/
│   ├── events.json
│   └── feedbacks.json
└── Program.cs
```

## Quick Start

```bash
cd FeedbackApp/FeedbackApp
dotnet run
```

Open: **http://localhost:5000**

## Admin Login

| Username | Password |
|----------|----------|
| `admin` | `admin123` |

## How It Works

### 1. Admin Creates Event
- Login to admin dashboard
- Click "Create New Event"
- Fill in event details
- Toggle "Include Star Rating" if needed
- Add custom questions using the Question Builder
- Save and get the shareable link

### 2. Share Link with Users
- Copy the unique link from dashboard
- Share via email, chat, or any medium
- Link format: `https://yoursite.com/Feedback/Submit/abc123def456`

### 3. Users Submit Feedback
- Users open the link directly
- Fill in the form (name optional)
- Rate with stars (if enabled)
- Answer custom questions
- Submit feedback

### 4. Admin Views Responses
- Go to Dashboard → View event
- See all responses with answers
- Delete individual feedback if needed

## Question Types

| Type | Description | Use Case |
|------|-------------|----------|
| **Short Text** | Single line input | Names, short answers |
| **Long Text** | Multi-line textarea | Detailed comments |
| **Single Choice** | Radio buttons | Pick one option |
| **Multiple Choice** | Checkboxes | Select multiple options |
| **Yes/No** | Toggle buttons | Binary questions |
