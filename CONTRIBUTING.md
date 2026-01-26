# Contributing to LT Messages

Thank you for your interest in contributing to LT Messages! This document provides guidelines and instructions for contributing.

## How to Contribute

### Reporting Issues
- Use the [GitHub Issues](https://github.com/senzal/LTMessages/issues) page
- Check if the issue already exists before creating a new one
- Provide detailed information:
  - Steps to reproduce
  - Expected behavior
  - Actual behavior
  - Blish HUD version
  - Module version
  - Screenshots if applicable

### Suggesting Features
- Open an issue with the "enhancement" label
- Describe the feature and its use case
- Explain how it would benefit LT/Commanders

### Pull Requests
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Test thoroughly in-game
5. Update documentation (README, comments)
6. Increment version in `manifest.json`
7. Commit with clear messages (`git commit -m 'Add amazing feature'`)
8. Push to your branch (`git push origin feature/amazing-feature`)
9. Open a Pull Request

## Development Setup

### Prerequisites
- .NET Framework 4.8 SDK
- Visual Studio 2019+ or VS Code with C# extension
- Blish HUD 1.2.0+
- Guild Wars 2

### Getting Started
```bash
git clone https://github.com/senzal/LTMessages.git
cd LTMessages
dotnet restore
dotnet build
```

### Development Workflow
1. Make changes to the code
2. Build: `dotnet build`
3. Exit Blish HUD
4. Copy `.bhm` to: `Documents\Guild Wars 2\addons\blishhud\modules\`
5. Start Blish HUD and test in GW2

## Code Guidelines

### Style
- Follow existing code style and conventions
- Use meaningful variable and method names
- Add XML documentation comments for public methods
- Keep methods focused and reasonably sized

### Best Practices
- Test all changes in-game before submitting
- Handle exceptions appropriately
- Log important events and errors
- Validate user input
- Maintain backwards compatibility when possible

### Version Numbering
Follow semantic versioning (MAJOR.MINOR.PATCH):
- **MAJOR**: Breaking changes
- **MINOR**: New features, backwards compatible
- **PATCH**: Bug fixes, backwards compatible

Update version in:
- `manifest.json`
- `README.md` changelog
- Session notes (if applicable)

## Testing

### Manual Testing Checklist
- [ ] Module loads without errors
- [ ] Popup shows correctly
- [ ] Messages send to squad chat
- [ ] Both chat methods work (Shift+Enter and Shift+/)
- [ ] In-game editor functions properly
- [ ] LT Mode toggle works
- [ ] Settings save and load correctly
- [ ] File auto-reload works
- [ ] No console errors in Blish HUD logs

### Test Scenarios
1. **Fresh install**: Delete messages.txt, test embedded defaults
2. **Message editing**: Add/edit/delete via editor, verify save
3. **External editing**: Edit messages.txt externally, verify reload
4. **LT Mode**: Test with enabled and disabled
5. **Chat methods**: Test both Shift+Enter and Shift+/
6. **Edge cases**: Empty messages, max length, special characters

## Documentation

### What to Update
When adding features or making changes:
- [ ] Update README.md
- [ ] Update inline code comments
- [ ] Update session notes if significant
- [ ] Add to CHANGELOG section in README
- [ ] Update this CONTRIBUTING.md if process changes

## Questions?

- Open an issue for questions
- Join [Blish HUD Discord](https://discord.gg/FYKN3qh)
- Tag maintainers in issues/PRs for urgent matters

## Code of Conduct

- Be respectful and constructive
- Focus on the issue, not the person
- Welcome newcomers
- Give credit where it's due
- Follow the Guild Wars 2 community guidelines

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to LT Messages! 🎉
