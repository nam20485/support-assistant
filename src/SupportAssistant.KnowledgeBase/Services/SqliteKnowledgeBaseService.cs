using Microsoft.Extensions.Logging;
using SupportAssistant.Core.Models;
using SupportAssistant.Core.Interfaces;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace SupportAssistant.KnowledgeBase.Services;

/// <summary>
/// SQLite-based knowledge base service implementation
/// </summary>
public class SqliteKnowledgeBaseService : IKnowledgeBaseService, IDisposable
{
    private readonly ILogger<SqliteKnowledgeBaseService> _logger;
    private SqliteConnection? _connection;
    private bool _isInitialized;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the SqliteKnowledgeBaseService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public SqliteKnowledgeBaseService(ILogger<SqliteKnowledgeBaseService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public bool IsReady => _isInitialized && _connection != null;

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            _logger.LogInformation("Knowledge Base Service is already initialized");
            return;
        }

        try
        {
            _logger.LogInformation("Initializing Knowledge Base Service...");

            var dbPath = GetDatabasePath();
            var connectionString = $"Data Source={dbPath}";
            
            _connection = new SqliteConnection(connectionString);
            await _connection.OpenAsync(cancellationToken);

            await CreateTablesAsync(cancellationToken);
            await SeedInitialDataAsync(cancellationToken);

            _isInitialized = true;
            _logger.LogInformation("Knowledge Base Service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Knowledge Base Service");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<KnowledgeSource>> SearchAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default)
    {
        if (!IsReady)
        {
            _logger.LogWarning("Knowledge Base Service is not ready");
            return new List<KnowledgeSource>();
        }

        try
        {
            _logger.LogInformation("Searching knowledge base for: {Query}", query);

            var results = new List<KnowledgeSource>();

            // Simple text search for now - in a real implementation, this would use vector similarity
            var sql = @"
                SELECT title, content, url, metadata 
                FROM knowledge_chunks 
                WHERE content LIKE @query OR title LIKE @query 
                ORDER BY 
                    CASE 
                        WHEN title LIKE @query THEN 1 
                        ELSE 2 
                    END,
                    length(content)
                LIMIT @maxResults";

            await using var command = new SqliteCommand(sql, _connection);
            command.Parameters.AddWithValue("@query", $"%{query}%");
            command.Parameters.AddWithValue("@maxResults", maxResults);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            
            while (await reader.ReadAsync(cancellationToken))
            {
                var source = new KnowledgeSource
                {
                    Title = reader.GetString(0),
                    Url = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Excerpt = TruncateContent(reader.GetString(1), 200),
                    RelevanceScore = CalculateSimpleRelevanceScore(query, reader.GetString(1))
                };

                results.Add(source);
            }

            _logger.LogInformation("Found {Count} knowledge sources for query", results.Count);
            return results.OrderByDescending(r => r.RelevanceScore).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching knowledge base");
            return new List<KnowledgeSource>();
        }
    }

    /// <inheritdoc />
    public async Task AddContentAsync(string content, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default)
    {
        if (!IsReady)
        {
            _logger.LogWarning("Knowledge Base Service is not ready");
            return;
        }

        try
        {
            var title = ExtractTitleFromContent(content);
            var metadataJson = metadata != null ? JsonSerializer.Serialize(metadata) : null;

            var sql = @"
                INSERT INTO knowledge_chunks (title, content, metadata, created_at)
                VALUES (@title, @content, @metadata, @createdAt)";

            await using var command = new SqliteCommand(sql, _connection);
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@content", content);
            command.Parameters.AddWithValue("@metadata", metadataJson ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("Added content to knowledge base: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding content to knowledge base");
            throw;
        }
    }

    private async Task CreateTablesAsync(CancellationToken cancellationToken)
    {
        var sql = @"
            CREATE TABLE IF NOT EXISTS knowledge_chunks (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                content TEXT NOT NULL,
                url TEXT,
                metadata TEXT,
                created_at DATETIME NOT NULL,
                updated_at DATETIME
            );

            CREATE INDEX IF NOT EXISTS idx_knowledge_content ON knowledge_chunks(content);
            CREATE INDEX IF NOT EXISTS idx_knowledge_title ON knowledge_chunks(title);
        ";

        await using var command = new SqliteCommand(sql, _connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task SeedInitialDataAsync(CancellationToken cancellationToken)
    {
        // Check if we already have data
        var countSql = "SELECT COUNT(*) FROM knowledge_chunks";
        await using var countCommand = new SqliteCommand(countSql, _connection);
        var existingCount = (long?)await countCommand.ExecuteScalarAsync(cancellationToken) ?? 0;

        if (existingCount > 0)
        {
            _logger.LogInformation("Knowledge base already contains {Count} entries", existingCount);
            return;
        }

        _logger.LogInformation("Seeding initial knowledge base data...");

        var sampleData = new[]
        {
            new { 
                Title = "Windows Update Troubleshooting", 
                Content = "To troubleshoot Windows Update issues: 1. Run Windows Update Troubleshooter 2. Reset Windows Update components 3. Check for corrupted system files using SFC scan 4. Clear Windows Update cache 5. Restart Windows Update service",
                Url = "https://docs.microsoft.com/en-us/windows/deployment/update/windows-update-troubleshooting"
            },
            new { 
                Title = "Blue Screen of Death (BSOD) Analysis", 
                Content = "When encountering a BSOD: 1. Note the error code and message 2. Check recently installed hardware/software 3. Run memory diagnostic 4. Update device drivers 5. Check system logs in Event Viewer 6. Consider system restore",
                Url = "https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/bug-check-code-reference"
            },
            new { 
                Title = "Network Connectivity Issues", 
                Content = "To resolve network connectivity problems: 1. Run network troubleshooter 2. Reset network adapters 3. Flush DNS cache 4. Reset TCP/IP stack 5. Check firewall settings 6. Update network drivers 7. Verify IP configuration",
                Url = "https://docs.microsoft.com/en-us/windows/client-management/troubleshoot-networking"
            },
            new { 
                Title = "Performance Optimization", 
                Content = "To improve system performance: 1. Disable startup programs 2. Clean temporary files 3. Check disk space 4. Run disk cleanup 5. Defragment hard drives 6. Check for malware 7. Update drivers 8. Adjust visual effects",
                Url = "https://docs.microsoft.com/en-us/windows/client-management/optimize-windows-10"
            }
        };

        foreach (var item in sampleData)
        {
            await AddContentAsync(item.Content, new Dictionary<string, object>
            {
                ["title"] = item.Title,
                ["url"] = item.Url,
                ["source"] = "Microsoft Docs"
            }, cancellationToken);
        }

        _logger.LogInformation("Seeded {Count} initial knowledge base entries", sampleData.Length);
    }

    private string GetDatabasePath()
    {
        var dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SupportAssistant");
        Directory.CreateDirectory(dataDir);
        return Path.Combine(dataDir, "knowledge.db");
    }

    private string ExtractTitleFromContent(string content)
    {
        // Simple title extraction - take first sentence or first 50 characters
        var sentences = content.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (sentences.Length > 0)
        {
            var firstSentence = sentences[0].Trim();
            return firstSentence.Length > 50 ? firstSentence.Substring(0, 50) + "..." : firstSentence;
        }

        return content.Length > 50 ? content.Substring(0, 50) + "..." : content;
    }

    private string TruncateContent(string content, int maxLength)
    {
        if (content.Length <= maxLength)
            return content;

        var truncated = content.Substring(0, maxLength);
        var lastSpace = truncated.LastIndexOf(' ');
        if (lastSpace > 0)
            truncated = truncated.Substring(0, lastSpace);

        return truncated + "...";
    }

    private float CalculateSimpleRelevanceScore(string query, string content)
    {
        // Simple relevance scoring based on keyword matches
        var queryWords = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var contentWords = content.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var matches = queryWords.Count(qw => contentWords.Any(cw => cw.Contains(qw)));
        return (float)matches / queryWords.Length;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _connection?.Dispose();
            _disposed = true;
            _logger.LogInformation("Knowledge Base Service disposed");
        }
    }
}