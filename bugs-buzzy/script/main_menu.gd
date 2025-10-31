extends Control

@onready var team_dialog: AcceptDialog = $VBoxContainer/TeamDialog
@onready var team_input: LineEdit = $VBoxContainer/TeamDialog/TeamInput
@onready var error_label: Label = $VBoxContainer/ErrorLabel

func _ready() -> void:
	error_label.text = ""  # Hide error text at start

	$VBoxContainer/PlayAsTeamMember.pressed.connect(_on_team_player_pressed)
	$VBoxContainer/Setting.pressed.connect(_on_settings_pressed)
	$VBoxContainer/TeamDialog/Ok.pressed.connect(_on_team_confirmed)


func _on_team_player_pressed() -> void:
	error_label.text = ""  # Clear old error
	team_dialog.popup_centered()
	team_input.grab_focus()


func _on_team_confirmed() -> void:
	var team_number := team_input.text.strip_edges()
	if team_number.is_empty():
		_show_error("⚠ Please enter a team number.")
		return

	print("Playing as team:", team_number)
	_start_game(team_number)


func _on_settings_pressed() -> void:
	print("Open settings menu")
	# Example: get_tree().change_scene_to_file("res://SettingsMenu.tscn")


func _start_game(team_id: String) -> void:
	error_label.text = ""
	print("Starting game with ID:", team_id)
	
	var game_scene = preload("res://scense/game2.tscn")
	get_tree().change_scene_to_packed(game_scene)


func _show_error(message: String) -> void:
	error_label.text = message
	error_label.add_theme_color_override("font_color", Color.RED)
