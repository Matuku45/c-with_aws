using Amazon.S3;
using Amazon.S3.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

var builder = WebApplication.CreateBuilder(args);

// S3 setup
string bucketName = "thabiso-unique-bucket-987654321";
var s3Client = new AmazonS3Client();  // Default credentials & region

// DynamoDB setup
string dynamoTableName = "MyTable";
var dynamoClient = new AmazonDynamoDBClient(); // Default credentials & region

var app = builder.Build();

// Serve your existing HTML UI (S3 browser UI)
app.MapGet("/", () =>
{
    return Results.Content(@"
<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='UTF-8' />
<meta name='viewport' content='width=device-width, initial-scale=1' />
<title>S3 Bucket and User Management</title>
<style>
  body {
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    margin: 2rem auto;
    max-width: 800px;
    background: #f0f4f8;
    color: #222;
    padding: 1rem 2rem;
    box-shadow: 0 0 12px rgb(0 0 0 / 0.1);
    border-radius: 12px;
  }
  h1 {
    color: #1a73e8;
    font-weight: 700;
    font-size: 2.2rem;
    margin-bottom: 1.5rem;
    text-align: center;
    letter-spacing: 1px;
  }
  button {
    background-color: #1a73e8;
    border: none;
    border-radius: 6px;
    padding: 0.5rem 1.2rem;
    color: white;
    font-weight: 600;
    font-size: 1rem;
    cursor: pointer;
    margin-right: 1rem;
    margin-top: 1rem;
    box-shadow: 0 3px 7px rgb(26 115 232 / 0.5);
    transition: background-color 0.3s ease;
  }
  button:hover {
    background-color: #155ab6;
  }
  #file-list, #user-list {
    margin-top: 1rem;
    background: white;
    border-radius: 8px;
    padding: 1rem 1.5rem;
    box-shadow: 0 4px 10px rgb(0 0 0 / 0.1);
    min-height: 200px;
    font-size: 1.1rem;
    color: #444;
  }
  .file-item, .user-item {
    padding: 0.8rem 0;
    border-bottom: 1px solid #e2e8f0;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  .file-item:last-child, .user-item:last-child {
    border-bottom: none;
  }
  form {
    margin-top: 2rem;
    background: white;
    padding: 1rem 1.5rem;
    border-radius: 8px;
    box-shadow: 0 4px 10px rgb(0 0 0 / 0.1);
    max-width: 400px;
  }
  label {
    display: block;
    margin-top: 0.8rem;
    font-weight: 600;
    color: #333;
  }
  input[type='text'] {
    width: 100%;
    padding: 0.4rem 0.6rem;
    font-size: 1rem;
    margin-top: 0.3rem;
    border-radius: 4px;
    border: 1px solid #ccc;
  }
  #message {
    margin-top: 1rem;
    font-weight: 600;
  }
</style>
</head>
<body>

<h1>S3 Bucket and User Management</h1>

<div>
  <button onclick='loadFiles()'>Load Files</button>
  <button onclick='loadUsers()'>Load Users</button>
</div>

<div id='file-list'></div>

<div id='user-list'></div>

<form id='add-user-form' onsubmit='event.preventDefault(); addUser();'>
  <h3>Add New User</h3>
  <label for='user-key'>User Key (12):</label>
  <input type='text' id='user-key' required />
  <label for='user-name'>User Name:</label>
  <input type='text' id='user-name' required />
  <button type='submit'>Add User</button>
</form>

<div id='message'></div>

<script>
async function loadFiles() {
  const listDiv = document.getElementById('file-list');
  const userDiv = document.getElementById('user-list');
  userDiv.innerHTML = ''; // Clear users
  listDiv.innerHTML = 'Loading files...';
  try {
    const res = await fetch('/list');
    if (!res.ok) throw new Error('Failed to load files');
    const files = await res.json();
    if (files.length === 0) {
      listDiv.innerHTML = '<p>No files found in bucket.</p>';
      return;
    }
    listDiv.innerHTML = '';
    files.forEach(file => {
      const div = document.createElement('div');
      div.className = 'file-item';
      div.textContent = file;

      const btn = document.createElement('button');
      btn.textContent = 'Download';
      btn.onclick = () => {
        window.open(`/download/${encodeURIComponent(file)}`, '_blank');
      };

      div.appendChild(btn);
      listDiv.appendChild(div);
    });
  } catch (error) {
    listDiv.innerHTML = `<p style='color: red;'>Error: ${error.message}</p>`;
  }
}

async function loadUsers() {
  const userDiv = document.getElementById('user-list');
  const listDiv = document.getElementById('file-list');
  listDiv.innerHTML = ''; // Clear files
  userDiv.innerHTML = 'Loading users...';
  try {
    // Scan DynamoDB table for all users (You might want to limit or paginate)
    const res = await fetch('/list-users');
    if (!res.ok) throw new Error('Failed to load users');
    const users = await res.json();
    if (users.length === 0) {
      userDiv.innerHTML = '<p>No users found in DynamoDB.</p>';
      return;
    }
    userDiv.innerHTML = '';
    users.forEach(user => {
      const div = document.createElement('div');
      div.className = 'user-item';
      div.textContent = `Key: ${user['12']} — Name: ${user['Name']}`;
      userDiv.appendChild(div);
    });
  } catch (error) {
    userDiv.innerHTML = `<p style='color: red;'>Error: ${error.message}</p>`;
  }
}

async function addUser() {
  const messageDiv = document.getElementById('message');
  messageDiv.textContent = '';
  const key = document.getElementById('user-key').value.trim();
  const name = document.getElementById('user-name').value.trim();
  if (!key || !name) {
    alert('Both key and name are required.');
    return;
  }
  try {
    const res = await fetch('/add-user', {
      method: 'POST',
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify({ '12': key, 'Name': name })
    });
    if (!res.ok) throw new Error('Failed to add user');
    messageDiv.style.color = 'green';
    messageDiv.textContent = 'User added successfully.';
    document.getElementById('add-user-form').reset();
    loadUsers();
  } catch (err) {
    messageDiv.style.color = 'red';
    messageDiv.textContent = `Error adding user: ${err.message}`;
  }
}

// Initialize with files loaded
loadFiles();
</script>

</body>
</html>
", "text/html");
});

// List S3 objects
app.MapGet("/list", async () =>
{
    try
    {
        var listResponse = await s3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = bucketName,
            MaxKeys = 1000
        });

        var keys = listResponse.S3Objects.Select(o => o.Key).ToList();
        return Results.Json(keys);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error listing objects: {ex.Message}", statusCode: 500);
    }
});

// Download from S3
app.MapGet("/download/{key}", async (string key) =>
{
    try
    {
        var response = await s3Client.GetObjectAsync(bucketName, key);
        return Results.File(response.ResponseStream, response.Headers.ContentType ?? "application/octet-stream", key);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving object: {ex.Message}", statusCode: 500);
    }
});

// ============ DynamoDB User Management ============

// Add user
app.MapPost("/add-user", async (HttpRequest request) =>
{
  try
  {
    var data = await request.ReadFromJsonAsync<Dictionary<string, string>>();
    if (data == null || !data.ContainsKey("12") || !data.ContainsKey("Name"))
      return Results.BadRequest("Missing required fields '12' or 'Name'.");

    var putItemRequest = new PutItemRequest
    {
      TableName = dynamoTableName,
      Item = new Dictionary<string, AttributeValue>
            {
                { "12", new AttributeValue { S = data["12"] } },
                { "Name", new AttributeValue { S = data["Name"] } }
            }
    };

    await dynamoClient.PutItemAsync(putItemRequest);
    return Results.Ok("User added successfully.");
  }
  catch (Exception ex)
  {
    return Results.Problem($"Error adding user: {ex.Message}", statusCode: 500);
  }
});

// List all users (Scan DynamoDB table)
app.MapGet("/list-users", async () =>
{
    try
    {
        var scanRequest = new ScanRequest
        {
            TableName = dynamoTableName
        };

        var response = await dynamoClient.ScanAsync(scanRequest);

        var users = response.Items.Select(item => item.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.S ?? ""
        )).ToList();

        return Results.Json(users);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error listing users: {ex.Message}", statusCode: 500);
    }
});




// Get user by key
app.MapGet("/get-user/{key}", async (string key) =>
{
    try
    {
        var getItemRequest = new GetItemRequest
        {
            TableName = dynamoTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "12", new AttributeValue { S = key } }
            }
        };

        var response = await dynamoClient.GetItemAsync(getItemRequest);

        if (response.Item == null || response.Item.Count == 0)
            return Results.NotFound("User not found.");

        // Convert AttributeValues to simple dictionary
        var result = response.Item.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.S ?? ""
        );

        return Results.Json(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving user: {ex.Message}", statusCode: 500);
    }
});

app.Run();
